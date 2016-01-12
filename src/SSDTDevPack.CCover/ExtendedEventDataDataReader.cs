using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Threading;

namespace SSDTDevPacl.CodeCoverage.Lib
{
    public class ExtendedEventDataDataReader
    {
        private readonly string _connectionString;

        public readonly ConcurrentQueue<CoveredStatement> CoveredStatements = new ConcurrentQueue<CoveredStatement>();
        public readonly ConcurrentDictionary<int, string> ObjectNameCache = new ConcurrentDictionary<int, string>();

        private bool _continue = true;

        private string _databaseName;

        public ExtendedEventDataDataReader(string connectionString)
        {
            _connectionString = connectionString;

        }

        public void Stop()
        {
            _gateway.StopTrace();

            foreach (var item in _gateway.GetStatements(this.ObjectNameCache))
            {
                CoveredStatements.Enqueue(item);
            }

        }

        private ExtendedEventGateway _gateway;

        public void Start()
        {
            _continue = true;
            _gateway = new DatabaseGateway(_connectionString).Get();

            _gateway.StartTrace();
            //var masterConnectionString = _connectionString.Replace(_databaseName, "master");

            //var events = new QueryableXEventData(masterConnectionString, CodeCoverageName, EventStreamSourceOptions.EventStream, EventStreamCacheOptions.DoNotCache);




            //try
            //{
            //    foreach (var evt in events)
            //    {
            //        if (evt.Name == "sp_statement_completed")
            //        {
            //            var covered = new CoveredStatement();
            //            covered.Offset = (int) evt.Fields["offset"].Value/2;
            //            var length = (int) evt.Fields["offset_end"].Value;
            //            if (length == -1)
            //                covered.Length = -1;
            //            else
            //                covered.Length = length/2 - covered.Offset;

            //            covered.Object = (string) evt.Fields["object_name"].Value;
            //            covered.ObjectId = (int) evt.Fields["object_id"].Value;
            //            covered.TimeStamp = evt.Timestamp;
            //            covered.ObjectType = ((MapValue) evt.Fields["object_type"].Value).Value;
            //            CoveredStatements.Enqueue(covered);
            //        }

            //        if (evt.Name == "module_start")
            //        {
            //            var objectId = (int) evt.Fields["object_id"].Value;
            //            var objectName = (string) evt.Fields["object_name"].Value;
            //            if (!ObjectNameCache.ContainsKey(objectId))
            //            {
            //                var schema = RunQueryWithValue(string.Format("select object_schema_name({0})", objectId));
            //                ObjectNameCache[objectId] = string.Format("{0}.{1}", schema, objectName);
            //            }
            //        }
            //    }
            //}
            //catch (Exception edddd)
            //{
            //    Console.WriteLine(edddd.Message);
            //}
        }
    }
}