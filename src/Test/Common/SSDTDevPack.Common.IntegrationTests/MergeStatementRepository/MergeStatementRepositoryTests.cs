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

        [Test]
        public void does_not_build_merge_for_table_not_in_dacpac()
        {
            var tableRepository = new TableRepository(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\bin\Debug\Nested.dacpac"));
            var mergeRepository = new MergeStatementRepository(tableRepository, Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\ABC\DEF\Script.PostDeploy.sql"));
            var merge = mergeRepository.Get().FirstOrDefault(p => p.Name.Value == "NotInDacpac");
            Assert.IsNull(merge);
        }

        [Test]
        public void does_not_build_merge_for_table_with_no_inline_table()
        {
            var tableRepository = new TableRepository(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\bin\Debug\Nested.dacpac"));
            var mergeRepository = new MergeStatementRepository(tableRepository, Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\ABC\DEF\Script.PostDeploy.sql"));
            var merge = mergeRepository.Get().FirstOrDefault(p => p.Name.Value == "NoInlineTable");
            Assert.IsNull(merge);
        }

        [Test]
        public void does_build_merge_for_table_with_no_schema_defined()
        {
            var tableRepository = new TableRepository(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\bin\Debug\Nested.dacpac"));
            var mergeRepository = new MergeStatementRepository(tableRepository, Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\ABC\DEF\Script.PostDeploy.sql"));
            var merge = mergeRepository.Get().FirstOrDefault(p => p.Name.Value == "NoSchema");
            Assert.IsNotNull(merge);
        }


        [Test]
        public void file_offset_and_length_are_correct()
        {
            var tableRepository = new TableRepository(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\bin\Debug\Nested.dacpac"));
            var mergeRepository = new MergeStatementRepository(tableRepository, Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\ABC\DEF\Script.PostDeploy.sql"));
            var merge = mergeRepository.Get().FirstOrDefault(p => p.Name.Value == "TheTable");

            using (var reader = new StreamReader(merge.ScriptDescriptor.FilePath))
            {
                var script = reader.ReadToEnd().Substring(merge.ScriptDescriptor.ScriptOffset, merge.ScriptDescriptor.ScriptLength);
                Assert.AreEqual(@"MERGE INTO dbo.TheTable
 AS TARGET
USING (VALUES (1, 'Ed', 1), (2, 'Ian', 0)) AS SOURCE(Id, name, fun) ON SOURCE.[Id] = TARGET.[Id]
WHEN NOT MATCHED BY TARGET THEN INSERT (Id, name, fun) VALUES (SOURCE.Id, SOURCE.name, SOURCE.fun)
WHEN MATCHED AND (NULLIF (SOURCE.fun, TARGET.fun) IS NOT NULL
                  OR NULLIF (SOURCE.name, TARGET.name) IS NOT NULL
                  OR NULLIF (SOURCE.Id, TARGET.Id) IS NOT NULL) THEN UPDATE 
SET TARGET.Id   = SOURCE.Id,
    TARGET.name = SOURCE.name,
    TARGET.fun  = SOURCE.fun
WHEN NOT MATCHED BY SOURCE THEN DELETE;", script);
            }
            
        }

        [Test]
        public void OriginalText_is_set_correctly()
        {
            var tableRepository = new TableRepository(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\bin\Debug\Nested.dacpac"));
            var mergeRepository = new MergeStatementRepository(tableRepository, Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\ABC\DEF\Script.PostDeploy.sql"));
            var merge = mergeRepository.Get().FirstOrDefault(p => p.Name.Value == "TheTable");

            
                Assert.AreEqual(@"MERGE INTO dbo.TheTable
 AS TARGET
USING (VALUES (1, 'Ed', 1), (2, 'Ian', 0)) AS SOURCE(Id, name, fun) ON SOURCE.[Id] = TARGET.[Id]
WHEN NOT MATCHED BY TARGET THEN INSERT (Id, name, fun) VALUES (SOURCE.Id, SOURCE.name, SOURCE.fun)
WHEN MATCHED AND (NULLIF (SOURCE.fun, TARGET.fun) IS NOT NULL
                  OR NULLIF (SOURCE.name, TARGET.name) IS NOT NULL
                  OR NULLIF (SOURCE.Id, TARGET.Id) IS NOT NULL) THEN UPDATE 
SET TARGET.Id   = SOURCE.Id,
    TARGET.name = SOURCE.name,
    TARGET.fun  = SOURCE.fun
WHEN NOT MATCHED BY SOURCE THEN DELETE;", merge.ScriptDescriptor.OriginalText);
            
        }

        [Test]
        public void ignores_other_statements_in_file()
        {
            var tableRepository = new TableRepository(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\bin\Debug\Nested.dacpac"));
            var mergeRepository = new MergeStatementRepository(tableRepository, Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\ABC\DEF\Script.NotIncluded.sql"));
            Assert.DoesNotThrow(() =>
            {
                mergeRepository.Get();
            });

            Assert.AreEqual(0, mergeRepository.Get().Count);
        }

        [Test]
        public void sets_merge_options_to_show_missing_update_clause()
        {
            var tableRepository = new TableRepository(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\bin\Debug\Nested.dacpac"));
            var mergeRepository = new MergeStatementRepository(tableRepository, Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\ABC\DEF\Script.PostDeploy.sql"));
            var merge = mergeRepository.Get().FirstOrDefault(p => p.Name.Value == "NoUpdate");
            Assert.IsFalse(merge.Option.HasUpdate);
            Assert.IsTrue(merge.Option.HasInsert);
            Assert.IsTrue(merge.Option.HasDelete);
        }

        [Test]
        public void sets_merge_options_to_show_missing_insert_clause()
        {
            var tableRepository = new TableRepository(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\bin\Debug\Nested.dacpac"));
            var mergeRepository = new MergeStatementRepository(tableRepository, Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\ABC\DEF\Script.PostDeploy.sql"));
            var merge = mergeRepository.Get().FirstOrDefault(p => p.Name.Value == "NoInsert");
            Assert.IsFalse(merge.Option.HasInsert);
            Assert.IsTrue(merge.Option.HasUpdate);
            Assert.IsTrue(merge.Option.HasDelete);
        }

        [Test]
        public void sets_merge_options_to_show_missing_delete_clause()
        {
            var tableRepository = new TableRepository(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\bin\Debug\Nested.dacpac"));
            var mergeRepository = new MergeStatementRepository(tableRepository, Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\ABC\DEF\Script.PostDeploy.sql"));
            var merge = mergeRepository.Get().FirstOrDefault(p => p.Name.Value == "NoDelete");
            Assert.IsFalse(merge.Option.HasDelete);
            Assert.IsTrue(merge.Option.HasInsert);
            Assert.IsTrue(merge.Option.HasUpdate);
        }
    }

}
