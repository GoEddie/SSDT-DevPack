using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using NUnit.Framework;
using SSDTDevPack.Common.VSPackage;

namespace SSDTDevPack.Common.IntegrationTests
{
    [TestFixture]
    public class ProjectEnumerator
    {
        [Test]
        [STAThread]
        [TestCase("VisualStudio.DTE.12.0")]
        [TestCase("VisualStudio.DTE.14.0")]
        public void finds_all_ssdt_projects_in_a_solution(string dteVersion)
        {
            try
            {
                var dte = (DTE) Activator.CreateInstance(Type.GetTypeFromProgID(dteVersion, true), true);
                Console.WriteLine(Directory.GetCurrentDirectory());
               
                dte.Solution.Open(new FileInfo(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\Nested.sln")) .FullName);
                VsServiceProvider.Register(new DteVsPackageProvider(dte));

                var projects = new Common.ProjectEnumerator().Get(null);
                Assert.AreEqual(2, projects.Count);

                try
                {
                    dte.Quit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error cleaning up: {0}", ex);
                }
                

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Assert.Fail();
            }
        }

    }

    class DteVsPackageProvider : IVsServiceProvider
    {
        private readonly DTE _dte;

        public DteVsPackageProvider(DTE dte)
        {
            _dte = dte;
        }

        public object GetVsService(Type type)
        {
            return _dte;
        }
    }

    static class Directories
    {
        public static string GetSampleSolution()
        {
            return @"..\..\..\SampleSolutions";
        }
    }
}
