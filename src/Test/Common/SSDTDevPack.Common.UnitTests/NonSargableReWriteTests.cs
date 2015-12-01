using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using SSDTDevPack.Indexes;

namespace SSDTDevPack.Common.UnitTests
{
    [TestFixture]
    public class NonSargableReWriteTests
    {

        [Test]
        public void sargable_rewrites_isnull_not_equals_different_literal()
        {

            var rewriter = new Replacements.NonSargableRewrites(@" select * from dbo.tableaaa
	        where isnull(a_column, 'sss') <> 'abc'
	   ");

            var replacements = rewriter.GetReplacements();
            Assert.AreEqual(1, replacements.Count);

            Assert.AreEqual("isnull(a_column, 'sss') <> 'abc'", replacements.FirstOrDefault().Original);
            Assert.AreEqual("(a_column is null\r\n or a_column <> 'abc')", replacements.FirstOrDefault().Replacement);


        }

        [Test]
        public void sargable_rewrites_isnull_equals_different_literal()
        {

            var rewriter = new Replacements.NonSargableRewrites(@" select * from dbo.tableaaa
	        where isnull(a.a_column, 'sss') = 'abc'
	   ");

            var replacements = rewriter.GetReplacements();
            Assert.AreEqual(1, replacements.Count);

            Assert.AreEqual("isnull(a.a_column, 'sss') = 'abc'", replacements.FirstOrDefault().Original);
            Assert.AreEqual("(a.a_column = 'abc')", replacements.FirstOrDefault().Replacement);
            
        }



        [Test]
        public void sargable_rewrites_isnull_not_equals_same_literal()
        {

            var rewriter = new Replacements.NonSargableRewrites(@" select * from dbo.tableaaa
	        where isnull(a.a_column, 'abc') <> 'abc'
	   ");

            var replacements = rewriter.GetReplacements();
            Assert.AreEqual(1, replacements.Count);

            Assert.AreEqual("isnull(a.a_column, 'abc') <> 'abc'", replacements.FirstOrDefault().Original);
            Assert.AreEqual("(a.a_column is not null\r\n and a.a_column <> 'abc')", replacements.FirstOrDefault().Replacement);


        }


        [Test]
        public void sargable_rewrites_isnull_equals_same_literal()
        {

            var rewriter = new Replacements.NonSargableRewrites(@" select * from dbo.tableaaa
	        where isnull(a.a_column, 'abc') = 'abc'
	   ");

            var replacements = rewriter.GetReplacements();
            Assert.AreEqual(1, replacements.Count);

            Assert.AreEqual("isnull(a.a_column, 'abc') = 'abc'", replacements.FirstOrDefault().Original);
            Assert.AreEqual("(a.a_column is null\r\n or a.a_column = 'abc')", replacements.FirstOrDefault().Replacement);


        }

       

    }
}
