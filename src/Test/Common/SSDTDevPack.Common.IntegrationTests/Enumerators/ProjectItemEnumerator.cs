using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using NUnit.Framework;
using SSDTDevPack.Common.ProjectItems;
using SSDTDevPack.Common.VSPackage;
using Thread = System.Threading.Thread;

namespace SSDTDevPack.Common.IntegrationTests
{
    [TestFixture]
    public class ProjectItemEnumerator
    {
        private DTE _dte;

        public bool Init(string dteVersion)
        {

            //Throw away DBML files otherwise it takes too long to poen the solution
            var file = Path.Combine(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\Nested.dbml"));
            while(File.Exists(file))
                File.Delete(file);
            
            file = Path.Combine(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested2\Nested2.dbml"));
            while(File.Exists(file))
                File.Delete(file);
            

            var dte = _dte = (DTE) Activator.CreateInstance(Type.GetTypeFromProgID(dteVersion, true), true);

            dte.MainWindow.Activate();
            dte.Solution.Open(
                new FileInfo(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\Nested.sln"))
                    .FullName);
            VsServiceProvider.Register(new DteVsPackageProvider(dte));
            MessageFilter.Register();

           // Thread.Sleep(30*1000);

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
        public void finds_all_files_in_a_project(string dteVersion)
        {
            try
            {
                Init(dteVersion);

                var files =
                    new Enumerators.ProjectItemEnumerator().Get(
                        new Enumerators.ProjectEnumerator().Get("{00d1a9c2-b5f0-4af3-8072-f6c62b433612}")
                            .FirstOrDefault(p => p.FileName == "Nested.sqlproj"));


                Assert.IsNotNull(files.FirstOrDefault(p => p.Name == "Script.Included.sql"));
                Assert.IsNotNull(files.FirstOrDefault(p => p.Name == "Script.NotIncluded.sql"));
                Assert.IsNotNull(files.FirstOrDefault(p => p.Name == "Script.PostDeploy.sql"));
                Assert.IsNotNull(files.FirstOrDefault(p => p.Name == "Script.Predeploy.sql"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Assert.Fail();
            }
        }

        [Test]
        [TestCase("VisualStudio.DTE.12.0")]
        [TestCase("VisualStudio.DTE.14.0")]
        public void finds_files_with_specific_build_action(string dteVersion)
        {
            try
            {
                Init(dteVersion);

                var files =
                    new Enumerators.ProjectItemEnumerator().Get(
                        new Enumerators.ProjectEnumerator().Get("{00d1a9c2-b5f0-4af3-8072-f6c62b433612}")
                            .FirstOrDefault(p => p.FileName == "Nested.sqlproj"));
                Assert.IsNotNull(
                    files.FirstOrDefault(p => p.HasBuildAction("PostDeploy") && p.Name == "Script.PostDeploy.sql"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Assert.Fail();
            }
        }
    }
}