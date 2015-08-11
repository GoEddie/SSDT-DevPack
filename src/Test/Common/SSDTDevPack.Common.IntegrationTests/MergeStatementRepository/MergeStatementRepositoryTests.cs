using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Merge.Parsing;

namespace SSDTDevPack.Common.IntegrationTests.MergeStatementRepositoryTests
{
    [TestFixture]
    public class MergeStatementRepositoryTests
    {
        [Test]
        public void can_parse_generated_merge_statement()
        {
            var tableRepository = new TableRepository(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\bin\Debug\Nested.dacpac"));
            var mergeRepository = new MergeStatementRepository(tableRepository, Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\ABC\DEF\Script.PostDeploy.sql"));
            var merge = mergeRepository.Get().FirstOrDefault(p=>p.Name.Value == "TheTable");
            Assert.AreEqual(2, merge.Data.Rows.Count);
            Assert.AreEqual("Ed", merge.Data.Rows[0][1]);
            Assert.AreEqual("Ian", merge.Data.Rows[1][1]);
            Assert.IsTrue(merge.Option.HasDelete);
            Assert.IsTrue(merge.Option.HasUpdate);
            Assert.IsTrue(merge.Option.HasInsert);
            
        
        }
    }

}
