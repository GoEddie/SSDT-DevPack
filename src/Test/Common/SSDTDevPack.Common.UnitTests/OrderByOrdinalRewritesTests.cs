using System.Linq;
using NUnit.Framework;
using SSDTDevPack.Rewriter;

namespace SSDTDevPack.Common.UnitTests
{
    [TestFixture]
    public class OrderByOrdinalRewritesTests
    {
        [Test]
        public void DoesNotRewriteOrdinalsWithSelectStarBeforePosition()
        {
            var script = @" select *, abc from dbo.tableaaa
	        order by 1
	   ";
            var rewriter = new OrderByOrdinalRewrites();

            var replacements = rewriter.GetReplacements(ScriptDom.ScriptDom.GetQuerySpecifications(script));
            Assert.IsNull(replacements);
            
        }

        [Test]
        public void DoesRewriteOrdinalsWithSelectStarAfterPosition()
        {
            var script = @" select abc, *, abc from dbo.tableaaa
	        order by 1
	   ";
            var rewriter = new OrderByOrdinalRewrites();

            var replacements = rewriter.GetReplacements(ScriptDom.ScriptDom.GetQuerySpecifications(script));
            Assert.IsNotNull(replacements);
            Assert.AreEqual(1, replacements.Count);
            Assert.AreEqual("abc", replacements.FirstOrDefault().Replacement);

        }

        [Test]
        public void DoesRewriteOrdinals()
        {
            var script = @" select abc, def, abc from dbo.tableaaa
	        order by 2,/*jkjkjj*/3
	   ";
            var rewriter = new OrderByOrdinalRewrites();

            var replacements = rewriter.GetReplacements(ScriptDom.ScriptDom.GetQuerySpecifications(script));
            Assert.IsNotNull(replacements);
            Assert.AreEqual(2, replacements.Count);
            Assert.AreEqual("def", replacements.FirstOrDefault().Replacement);
            Assert.AreEqual("abc", replacements.LastOrDefault().Replacement);

        }
        [Test]
        public void DoesNotRewriteColumnReferences()
        {
            var script = @" select abc, def, abc from dbo.tableaaa
	        order by abc, def, /*jkjkjj*/
	   ";
            var rewriter = new OrderByOrdinalRewrites();

            var replacements = rewriter.GetReplacements(ScriptDom.ScriptDom.GetQuerySpecifications(script));
            Assert.AreEqual(0, replacements.Count);
        }
    }
}