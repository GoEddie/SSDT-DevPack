using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.ScriptDom;

namespace SSDTDevPack.Merge.Parsing
{
    class MergeStatementRepository
    {
        private readonly TableRepository _tables;
        private readonly string _path;

        public MergeStatementRepository(TableRepository tables, string path)
        {
            _tables = tables;
            _path = path;
        }

        public void Populate()
        {
            Merges = new List<MergeDescriptor.Merge>();
            var statementParser = new MergeStatementParser();

            var parser = new ScriptParser(statementParser, _path);
            parser.Parse();

            foreach (var statement in statementParser.Merges)
            {
                Console.WriteLine(statement);
                TODO PARSE THE MERGE AND GET THE BITS WE WANT - WHOOP WHOOOP
            }

        }

        public List<MergeDescriptor.Merge> Merges { get; set; }

    }
}
