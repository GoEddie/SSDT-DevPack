using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SSDTDevPack.Common.Enumerators.tSQLt;

namespace SSDTDevPack.Common.IntegrationTests.Enumerators.tSQLt
{

    [TestFixture]
    public class TestClassEnumeratorTests
    {

        [Test]
        public void a()
        {
            var enumerator = new TestClassEnumerator(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\bin\Debug\Nested.dacpac"));
        }

    }
}
