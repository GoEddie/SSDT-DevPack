using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Merge.MergeDescriptor;

namespace SSDTDevPack.Common.IntegrationTests.MergeWriter
{
    [TestFixture]
    public class MergeWriterTests
    {
        [Test]
        public void test()
        {
            var tableRepository = new TableRepository(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\bin\Debug\Nested.dacpac"));
            var m = new Merge.MergeDescriptor.Merge();

            m.Table = tableRepository.Get().First(p=>p.Name.GetName() == "TheTable");
            m.Data = new DataTable();
            foreach (var c in m.Table.Columns)
            {
                m.Data.Columns.Add(c.Name.GetName());
            }

            for (int i = 0; i < 10; i++)
            {
                var r = m.Data.NewRow();
                for (int j = 0; j < r.ItemArray.Length; j++)
                {
                    r[j] = i;
                }
                m.Data.Rows.Add(r);
            }
            m.Name = m.Table.Name.ToIdentifier();

            m.Option = new MergeOptions(true, true, false, false);


            var writer = new Merge.MergeDescriptor.MergeWriter(m);
            writer.Write();
        }
    }
}
