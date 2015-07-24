using System;
using System.IO;
using System.Linq;
using EnvDTE;
using NUnit.Framework;
using SSDTDevPack.Common.VSPackage;

namespace SSDTDevPack.Common.IntegrationTests
{
    [TestFixture]
    public class ProjectItemEnumerator
    {

        private DTE _dte;

        public void init(string dteVersion)
        {
            var dte = _dte = (DTE)Activator.CreateInstance(Type.GetTypeFromProgID(dteVersion, true), true);

            dte.MainWindow.Activate();
            dte.Solution.Open(new FileInfo(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\Nested.sln")).FullName);
            VsServiceProvider.Register(new DteVsPackageProvider(dte));
            MessageFilter.Register();
                
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
                init(dteVersion);

                var files = new Common.ProjectItemEnumerator().Get(new Common.ProjectEnumerator().Get("{00d1a9c2-b5f0-4af3-8072-f6c62b433612}").FirstOrDefault(p => p.FileName == "Nested.sqlproj"));

                foreach (var file in files)
                {
                    Console.WriteLine("file: " + file.Name);

                }
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

    }
}