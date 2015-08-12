using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;

namespace SSDTDevPack.Common.IntegrationTests.ScriptParser
{
    [TestFixture]
    public class ScriptParserTests
    {
        [Test]
        public void replaces_colon_import_sqlcmd_with_comment()
        {
            var path = Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\ABC\DEF\GoodImport.sql");
            var visitor = new DummyMergeVisitor();

            var scriptParser = new ScriptDom.ScriptParser(visitor, path);
            scriptParser.Parse();

            Assert.AreEqual(1, visitor.Merges.Count);
        }

        [Test]
        public void stops_parsing_when_colon_sqlcmd_line_is_less_than_2_chars_so_cant_be_replaced_with_comment()
        {
            var path = Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\ABC\DEF\BadImport.sql");
            var visitor = new DummyMergeVisitor();

            var scriptParser = new ScriptDom.ScriptParser(visitor, path);
            scriptParser.Parse();

            Assert.AreEqual(0, visitor.Merges.Count);
        }
    }


    public class DummyMergeVisitor : TSqlFragmentVisitor
    {
        public List<MergeStatement> Merges = new List<MergeStatement>();

        public override void ExplicitVisit(MergeStatement merge)
        {
            Merges.Add(merge);
        }
    }

}
