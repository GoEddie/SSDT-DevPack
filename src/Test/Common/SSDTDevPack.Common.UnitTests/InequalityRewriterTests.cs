using System.IO;
using NUnit.Framework;
using SSDTDevPack.Rewriter;

namespace SSDTDevPack.Common.UnitTests
{
    static class Directories
    {
        public static string GetSampleSolution()
        {
            return @"..\..\..\SampleSolutions";
        }
    }

    [TestFixture]
    public class InequalityRewriterTests
    {
        [Test]
        public void RewritesBangEquals()
        {
            var script = @"select 1, (select top 1 a from b where 1!=2) from a where a ! /*sssssss**/  =   b";

            var rewriter = new InEqualityRewriter(script);
            var replacements = rewriter.GetReplacements(ScriptDom.ScriptDom.GetQuerySpecifications(script));
            Assert.AreEqual(2, replacements.Count);
        }


        //[Test] - NOT SURE WHERE GoodImport.sql is??
        //public void MultipleRewritesAreFound()
        //{
        //    var script = File.ReadAllText(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\ABC\DEF\GoodImport.sql"));
        //    var rewriter = new InEqualityRewriter(script);
        //    var replacements = rewriter.GetReplacements(ScriptDom.ScriptDom.GetQuerySpecifications(script));

        //    Assert.AreEqual(2, replacements.Count);


        //}
    }
}