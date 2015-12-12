using System.Linq;
using NUnit.Framework;
using SSDTDevPack.Rewriter;

namespace SSDTDevPack.Common.UnitTests
{
    [TestFixture]
    public class NonSargableReWriteTests
    {
        [Test]
        public void sargable_rewrites_isnull_equals_different_literal()
        {
            var script = @" select * from dbo.tableaaa
	        where isnull(a.a_column, 'sss') = 'abc'
	   ";
            var rewriter = new NonSargableRewrites(script);

            var replacements = rewriter.GetReplacements(ScriptDom.ScriptDom.GetQuerySpecifications(script));
            Assert.AreEqual(1, replacements.Count);

            Assert.AreEqual("isnull(a.a_column, 'sss') = 'abc'", replacements.FirstOrDefault().Original);
            Assert.AreEqual("(a.a_column = 'abc')", replacements.FirstOrDefault().Replacement);
        }


        [Test]
        public void sargable_rewrites_isnull_equals_same_literal()
        {
            var script = @" select * from dbo.tableaaa
	        where isnull(a.a_column, 'abc') = 'abc'
	   ";
            var rewriter = new NonSargableRewrites(script);

            var replacements = rewriter.GetReplacements(ScriptDom.ScriptDom.GetQuerySpecifications(script));
            Assert.AreEqual(1, replacements.Count);

            Assert.AreEqual("isnull(a.a_column, 'abc') = 'abc'", replacements.FirstOrDefault().Original);
            Assert.AreEqual("(a.a_column is null or a.a_column = 'abc')", replacements.FirstOrDefault().Replacement);
        }

        [Test]
        public void sargable_rewrites_isnull_not_equals_different_literal()
        {
            var script = @" select * from dbo.tableaaa
	        where isnull(a_column, 'sss') <> 'abc'
	   ";
            var rewriter = new NonSargableRewrites(script);

            var replacements = rewriter.GetReplacements(ScriptDom.ScriptDom.GetQuerySpecifications(script));
            Assert.AreEqual(1, replacements.Count);

            Assert.AreEqual("isnull(a_column, 'sss') <> 'abc'", replacements.FirstOrDefault().Original);
            Assert.AreEqual("(a_column is null or a_column <> 'abc')", replacements.FirstOrDefault().Replacement);
        }


        [Test]
        public void sargable_rewrites_isnull_not_equals_same_literal()
        {
            var script = @" select * from dbo.tableaaa
	        where isnull(a.a_column, 'abc') <> 'abc'
	   ";
            var rewriter = new NonSargableRewrites(script);

            var replacements = rewriter.GetReplacements(ScriptDom.ScriptDom.GetQuerySpecifications(script));
            Assert.AreEqual(1, replacements.Count);

            Assert.AreEqual("isnull(a.a_column, 'abc') <> 'abc'", replacements.FirstOrDefault().Original);
            Assert.AreEqual("(a.a_column is not null and a.a_column <> 'abc')", replacements.FirstOrDefault().Replacement);
        }


        [Test]
        public void sargable_rewrites_isnulls_after_unions()
        {
            var script = @" select * from dbo.tableaaa
	        where isnull(a_column, 'sss') <> 'abc' union all select * from dbo.tableaaa
	        where isnull(a_column, 'sss') <> 'abc' 
	   ";
            var rewriter = new NonSargableRewrites(script);

            var replacements = rewriter.GetReplacements(ScriptDom.ScriptDom.GetQuerySpecifications(script));
            Assert.AreEqual(2, replacements.Count);

            Assert.AreEqual("isnull(a_column, 'sss') <> 'abc'", replacements.FirstOrDefault().Original);
            Assert.AreEqual("(a_column is null or a_column <> 'abc')", replacements.FirstOrDefault().Replacement);

            Assert.AreEqual("isnull(a_column, 'sss') <> 'abc'", replacements.LastOrDefault().Original);
            Assert.AreEqual("(a_column is null or a_column <> 'abc')", replacements.LastOrDefault().Replacement);
        }


        [Test]
        public void sargable_rewrites_isnulls_on_joins()
        {
            var script = @" select * from dbo.tableaaa join tableb on 
	        isnull(a_column, 'sss') <> 'abc' 	   ";
            var rewriter = new NonSargableRewrites(script);

            var replacements = rewriter.GetReplacements(ScriptDom.ScriptDom.GetQuerySpecifications(script));
            Assert.AreEqual(1, replacements.Count);

            Assert.AreEqual("isnull(a_column, 'sss') <> 'abc'", replacements.FirstOrDefault().Original);
            Assert.AreEqual("(a_column is null or a_column <> 'abc')", replacements.FirstOrDefault().Replacement);
        }
    }
}