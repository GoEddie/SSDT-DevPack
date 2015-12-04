using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.Settings;

namespace SSDTDevPack.QueryCosts
{
    //public class QueryCostRepository
    //{
    //    private readonly QueryCostStore _store;
        
    //    public QueryCostRepository(QueryCostStore store)
    //    {
    //        _store = store;
    //    }

    //    public List<Statement> GetCosts(string path)
    //    {
    //        throw new NotImplementedException();        
    //    }

    //}

    public class QueryCostDataGateway
    {
        private readonly string _connectionString;

        public QueryCostDataGateway() //for mocking
        {
            
        }

        public QueryCostDataGateway(string connectionString)
        {
            _connectionString = connectionString;
        }

        public virtual string GetPlanForQuery(string query)
        {
            IList<ParseError> errors;
            var fragment = new TSql120Parser(false).Parse(new StringReader(query), out errors );
            var visitor = new ProcedureVisitor();
            fragment.Accept(visitor);

            foreach (var proc in visitor.Procedures)
            {
                var procName = proc.ProcedureReference.Name;
                return GetPlanForProc(procName);
            }
            foreach (var func in visitor.Functions)
            {
                var procName = func.Name;
                return GetPlanForFunc(procName, func.Parameters);
            }



            return "";
        }

        private string GetPlanForFunc(SchemaObjectName procName, IList<ProcedureParameter> parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var c = connection.CreateCommand();
                c.CommandText = "SET SHOWPLAN_XML ON";
                c.ExecuteNonQuery();
                c.CommandText = "SET NOEXEC ON";
                c.ExecuteNonQuery();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = string.Format("select * from  {0}.{1}({2})",
                        procName.SchemaIdentifier.Value.Quote(), procName.BaseIdentifier.Value.Quote(),(GetArgsString(parameters)));

                    return cmd.ExecuteScalar() as string;
                }
            }
            throw new NotImplementedException();
        }

        private string GetArgsString(IList<ProcedureParameter> parameters)
        {
            var values = "";
            foreach (var p in parameters)
            {
                values += ", NULL";
            }

            if(values.Length > 0)
                return values.Substring(1);

            return "";
        }

        private string GetPlanForProc(SchemaObjectName procName)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var c = connection.CreateCommand();
                c.CommandText = "SET SHOWPLAN_XML ON";
                c.ExecuteNonQuery();
                c.CommandText = "SET NOEXEC ON";
                c.ExecuteNonQuery();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = string.Format("exec {0}.{1}",
                        procName.SchemaIdentifier.Value.Quote(), procName.BaseIdentifier.Value.Quote());

                    return cmd.ExecuteScalar() as string;
                }
            }

            return null;
        }

        public virtual string GetTextFromFile(string path)
        {
            return File.ReadAllText(path);
        }
    }

    

    class ProcedureVisitor : TSqlFragmentVisitor
    {
        public List<CreateProcedureStatement> Procedures = new List<CreateProcedureStatement>();
        public List<CreateFunctionStatement> Functions = new List<CreateFunctionStatement>();
        
        public override void Visit(CreateProcedureStatement node)
        {
            Procedures.Add(node);
        }

        public override void Visit(CreateFunctionStatement node)
        {
            Functions.Add(node);
        }
    }

    public class QueryCostStore
    {
        private readonly PlanParser _planParser;

        public QueryCostStore(PlanParser planParser)
        {
            _planParser = planParser;
        }

        public void AddStatements(string script, string filePath)
        {
            lock (_cache)
            {
                if (!_cache.ContainsKey(filePath))
                {
                    _cache[filePath] = new QueryCostCacheEntry();
                }

                var cacheEntry = _cache[filePath];

                //string checkSum = GetChecksum(filePath);

                //if (checkSum != cacheEntry.FileChecksum)
                //{
                    cacheEntry.Statements = BuildStatements(filePath);
                 //   cacheEntry.FileChecksum = checkSum;
                //}

                
            }
        }

        public List<Statement> GetStatements(string filePath)
        {
            if (_cache.ContainsKey(filePath))
                return _cache[filePath].Statements;

            return null;
        } 

        private List<Statement> BuildStatements(string filePath)
        {
            var statements = _planParser.GetStatements(File.ReadAllText(filePath));

            var f = new CreateFunctionStatement();
            
            return statements;
        }
    //TODO     COSTER only shows select outer query on inline tvf
        private string GetChecksum(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return md5.ComputeHash(stream).ToString();
                }
            }
        }

        private Dictionary<string, QueryCostCacheEntry> _cache = new Dictionary<string, QueryCostCacheEntry>(); 

    }

    public class PlanParser
    {
        private readonly QueryCostDataGateway _gateway;
        private readonly Settings _Settings;

        public PlanParser(QueryCostDataGateway gateway)
        {
            _gateway = gateway;
            _Settings = SavedSettings.Get();
        }

        public List<Statement> GetStatements(string filePath)
        {
            var plan = _gateway.GetPlanForQuery(filePath);

            if(string.IsNullOrEmpty(plan))
                return new List<Statement>();

            XDocument cpo = XDocument.Parse(plan);
            XNamespace ns = "http://schemas.microsoft.com/sqlserver/2004/07/showplan";
            var elements = cpo.Descendants(ns + "StmtSimple");

            var statements = (from element in elements
                              let text = element.Attribute("StatementText") == null ? "" : element.Attribute("StatementText").Value
                let cost = element.Attribute("StatementSubTreeCost") == null ? "0.0001" : element.Attribute("StatementSubTreeCost").Value 
            
                select new Statement()
                {
                    Band = GetBand(cost), Text = text.Trim(), Cost = cost
                }).ToList();

            return statements;
        }

        
        private CostBand GetBand(string costText)
        {
            decimal cost = decimal.Parse(costText, System.Globalization.NumberStyles.Float);
            
            
            if (cost < _Settings.Costs.Medium)
                return CostBand.Good;

            if (cost < _Settings.Costs.High)
                return CostBand.Medium;

            return CostBand.High;
            
        }
    }

    class QueryCostCacheEntry
    {
        public string FilePath;
        public string FileChecksum;
        public List<Statement> Statements = new List<Statement>();
    }

    public class Statement
    {
        public string Text;
        public CostBand Band;
        public string Cost;
    }

    public enum CostBand
    {
        Good,
        Medium,
        High
    }
}
