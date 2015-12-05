using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using SSDTDevPack.Rewriter;

namespace SSDTDevPack.Common.UnitTests
{
    [TestFixture]
    public class NonSargableReWriteTests
    {

        [Test]
        public void sargable_rewrites_isnull_not_equals_different_literal()
        {
            var script = @" select * from dbo.tableaaa
	        where isnull(a_column, 'sss') <> 'abc'
	   ";
            var rewriter = new NonSargableRewrites(script);

            var replacements = rewriter.GetReplacements( ScriptDom.ScriptDom.GetQuerySpecifications(script));
            Assert.AreEqual(1, replacements.Count);

            Assert.AreEqual("isnull(a_column, 'sss') <> 'abc'", replacements.FirstOrDefault().Original);
            Assert.AreEqual("(a_column is null\r\n or a_column <> 'abc')", replacements.FirstOrDefault().Replacement);


        }

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
        public void sargable_rewrites_isnull_not_equals_same_literal()
        {
            var script = @" select * from dbo.tableaaa
	        where isnull(a.a_column, 'abc') <> 'abc'
	   ";
            var rewriter = new NonSargableRewrites(script);

            var replacements = rewriter.GetReplacements(ScriptDom.ScriptDom.GetQuerySpecifications(script));
            Assert.AreEqual(1, replacements.Count);

            Assert.AreEqual("isnull(a.a_column, 'abc') <> 'abc'", replacements.FirstOrDefault().Original);
            Assert.AreEqual("(a.a_column is not null\r\n and a.a_column <> 'abc')", replacements.FirstOrDefault().Replacement);


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
            Assert.AreEqual("(a.a_column is null\r\n or a.a_column = 'abc')", replacements.FirstOrDefault().Replacement);


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
    }

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
