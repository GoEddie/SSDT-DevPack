using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SSDTDevPacl.CodeCoverage.Lib
{
    class DatabaseGateway
    {
        private readonly string _connectionString;
        private readonly ExtendedEventGateway _gateway;

        public DatabaseGateway(string connectionString)
        {
            _connectionString = connectionString;
            switch (GetVersion())
            {
                case SqlServerVersion.Sql100:
                    _gateway = new ExtendedEventGateway2008(_connectionString);
                    break;
            }
        }

        private SqlServerVersion GetVersion()
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select @@version";
                    var versionString = cmd.ExecuteScalar().ToString();

                    if(versionString.StartsWith("Microsoft SQL Server 2008"))
                            return SqlServerVersion.Sql100;
                    
                }
            }

            return SqlServerVersion.Sql110;
        }

        public ExtendedEventGateway Get()
        {
            return _gateway;
        }
        
    }

    abstract class ExtendedEventGateway
    {
        protected readonly string _connectionString;

        protected ExtendedEventGateway(string _connectionString)
        {
            this._connectionString = _connectionString;
            DropSession();
        }

        public abstract void StartTrace();
        public abstract void StopTrace();
        public abstract IEnumerable<CoveredStatement> GetStatements(ConcurrentDictionary<int, string> objectNameCache);

        protected const string SessionName = "CodeCoverage";

        protected void StopSession()
        {
            RunQuery(string.Format(@"ALTER EVENT SESSION [{0}]
	ON SERVER
	STATE=STOP", SessionName));
        }

        protected void StartSession()
        {
            RunQuery(string.Format(@"ALTER EVENT SESSION [{0}]
	ON SERVER
	STATE=START", SessionName));

        }


        private void DropSession()
        {
            RunQuery(string.Format(@"if exists(select * from sys.server_event_sessions where name = '{0}')
begin
	drop event session {0} on server
end
", SessionName));
        }

        protected string GetDatabaseId(string databaseName)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = string.Format("select db_id('{0}')", databaseName);
                    var id = cmd.ExecuteScalar().ToString();
                    return id;
                } 
            }
        }

        protected string GetDatabaseName()
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select db_name()";
                    return cmd.ExecuteScalar().ToString();
                }
            }
        }

        protected string GetLogDir()
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "EXEC xp_readerrorlog 0, 1, N'Logging SQL Server messages in file'";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader["Text"].ToString().Replace("Logging SQL Server messages in file '", "").Replace("'", "");
                        }
                    }
                }
            }

            return String.Empty;
        }
        

        protected void RunQuery(string query)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    class ExtendedEventGateway2008 : ExtendedEventGateway
    {
        private string logFilePath;
        public ExtendedEventGateway2008(string connectionString) : base(connectionString)
        { 
          
        }
        public override void StartTrace()
        {

            var databaseName = GetDatabaseName();
            //1 - get database_id
            var id = GetDatabaseId(databaseName);
            //2 - get log path
            var logPath = GetLogDir();
            //3 - start trace
            logPath = new FileInfo(logPath).DirectoryName;
            logFilePath = Path.Combine(logPath, "CodeCoverage" + Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", ""));
            
            RunQuery(string.Format(@"CREATE EVENT SESSION [{1}] ON SERVER 
ADD EVENT sqlserver.module_start(
    WHERE ([sqlserver].[database_id]={0})),
ADD EVENT sqlserver.sp_statement_completed(
    ACTION(sqlserver.tsql_stack)
    WHERE ([sqlserver].[database_id]={0}))
ADD TARGET package0.asynchronous_file_target(SET filename=N'{2}')
WITH (MAX_MEMORY=4096 KB,EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS,MAX_DISPATCH_LATENCY=1 SECONDS,MAX_EVENT_SIZE=0 KB,MEMORY_PARTITION_MODE=NONE,TRACK_CAUSALITY=OFF,STARTUP_STATE=OFF)
", id, SessionName, logFilePath + ".xel"));
            
            StartSession();
        }

        


        public override void StopTrace()
        {
            StopSession();
        }

        private string RunQueryWithValue(string query)
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                   
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = query;
                        return cmd.ExecuteScalar().ToString();
                    }
                }
            }
            catch (Exception e)
            {
                //munch munch munch
            }
            return string.Empty;
        }


        public override IEnumerable<CoveredStatement> GetStatements(ConcurrentDictionary<int, string> objectNameCache)
        {
            var statements = new List<CoveredStatement>();

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"SELECT
    object_name,
    event_data,
    file_name,
    file_offset
FROM sys.fn_xe_file_target_read_file(N'{0}*.xel', N'{0}*.xem', null, null)", logFilePath
);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var type = reader["object_name"].ToString();

                            var doc = XDocument.Parse(reader["event_data"].ToString());
                            int object_id;

                            var timestamp = doc.Root.Attribute("timestamp")?.Value;

                            switch (type)
                            {
                                case "sp_statement_completed":

                                    //tsql_stack
                                    var stack = (from node in doc.Element("event").Elements("action")
                                                       where (string)node.Attribute("name") == "tsql_stack"
                                                 select node.Value).FirstOrDefault();

                                    var stackDoc = XDocument.Parse(string.Format("<tsql>{0}</tsql>", stack));

                                    var tsqlNode = stackDoc.Element("tsql").Elements("frame").FirstOrDefault();

                                    var offset = Int32.Parse(tsqlNode.Attribute("offsetStart").Value)/2;
                                    var offsetEnd = Int32.Parse(tsqlNode.Attribute("offsetEnd").Value);

                                    object_id = (from node in doc.Element("event").Elements("data")
                                                     where (string)node.Attribute("name") == "object_id"
                                                     select Int32.Parse(node.Value)).FirstOrDefault();



                                    var length = offsetEnd;
                                    if (length > -1)
                                    {
                                        length = (length/2) - offset;
                                    }

                                    statements.Add(new CoveredStatement()
                                    {
                                        ObjectId = object_id,
                                        Offset = offset,
                                        Length =  length,
                                        TimeStamp =  DateTimeOffset.Parse(timestamp)
                                    });

                                    break;

                                case "module_start":
                                    
                                    

                        
                                    object_id = (from node in doc.Element("event").Elements("data")
                                        where (string) node.Attribute("name") == "object_id"
                                        select Int32.Parse(node.Value)).FirstOrDefault();


                                    var object_name = (from node in doc.Element("event").Elements("data")
                                                      where (string)node.Attribute("name") == "object_name"
                                                      select node.Value).FirstOrDefault();

                                    if (!objectNameCache.ContainsKey(object_id))
                                    {
                                        var schema = RunQueryWithValue(string.Format("select object_schema_name({0})", object_id));
                                        objectNameCache[object_id] = string.Format("{0}.{1}", schema, object_name);
                                    }

                                    break;
                            }

                            
                        }
                    }
                }
            }

            return statements;
        }
    }

}
