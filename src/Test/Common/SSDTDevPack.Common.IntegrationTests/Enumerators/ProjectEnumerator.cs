using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using NUnit.Framework;
using SSDTDevPack.Common.VSPackage;
using Thread = System.Threading.Thread;

namespace SSDTDevPack.Common.IntegrationTests
{
    [TestFixture]
    public class ProjectEnumerator
    {
        private DTE _dte;

        public bool Init(string dteVersion)
        {

            //Throw away DBML files otherwise it takes too long to poen the solution
            var file = Path.Combine(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\Nested.dbml"));
            while (File.Exists(file))
                File.Delete(file);

            file = Path.Combine(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested2\Nested2.dbml"));
            while (File.Exists(file))
                File.Delete(file);


            var dte = _dte = (DTE)Activator.CreateInstance(Type.GetTypeFromProgID(dteVersion, true), true);

            dte.MainWindow.Activate();
            dte.Solution.Open(
                new FileInfo(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\Nested.sln"))
                    .FullName);
            VsServiceProvider.Register(new DteVsPackageProvider(dte));
            MessageFilter.Register();

            Thread.Sleep(10 * 1000);

            while (true)
            {
                try
                {
                    return dte.Solution.Projects.Count > 0;
                }
                catch (COMException ce)
                {
                    if (ce.HResult != 0x8001010A)
                    {
                        throw;
                    }
                }
            }
        }


        [TestFixtureTearDown]
        public void Terminate()
        {
            try
            {
                _dte.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error cleaning up: {0}", ex);
            }

            MessageFilter.Revoke();
        }


        [Test]
        [STAThread]
        [TestCase("VisualStudio.DTE.12.0")]
        [TestCase("VisualStudio.DTE.14.0")]
        public void finds_all_ssdt_projects_in_a_solution(string dteVersion)
        {
            try
            {
                Init(dteVersion);
                
                var projects = new Enumerators.ProjectEnumerator().Get("{00d1a9c2-b5f0-4af3-8072-f6c62b433612}");
                
                try
                {
                    _dte.Quit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error cleaning up: {0}", ex);
                }
                
                MessageFilter.Revoke();

                Assert.AreEqual(2, projects.Count);
                Assert.IsNotNull(projects.FirstOrDefault(p=>p.FileName == "Nested.sqlproj"));
                Assert.IsNotNull(projects.FirstOrDefault(p => p.FileName == "Nested2.sqlproj"));

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Assert.Fail();
            }
        }

    }
}
