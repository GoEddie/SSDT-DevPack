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
                
               
                dte.Solution.Open(new FileInfo(Path.Combine(Directories.GetSampleSolution(), @"NestedProjects\Nested\Nested.sln")) .FullName);
                VsServiceProvider.Register(new DteVsPackageProvider(dte));
                MessageFilter.Register();
                // Display the Visual Studio IDE.
                dte.MainWindow.Activate();
                var projects = new Common.ProjectEnumerator().Get("{00d1a9c2-b5f0-4af3-8072-f6c62b433612}");
                
                try
                {
                    
                    dte.Quit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error cleaning up: {0}", ex);
                }
                
                MessageFilter.Revoke();

                Assert.AreEqual(2, projects.Count);
                Console.WriteLine("have 2 PROJECTS");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Assert.Fail();
            }
        }

    }

    public class MessageFilter : IOleMessageFilter
    {
        //
        // Class containing the IOleMessageFilter
        // thread error-handling functions.

        // Start the filter.
        public static void Register()
        {
            IOleMessageFilter newFilter = new MessageFilter();
            IOleMessageFilter oldFilter = null;
            CoRegisterMessageFilter(newFilter, out oldFilter);
        }

        // Done with the filter, close it.
        public static void Revoke()
        {
            IOleMessageFilter oldFilter = null;
            CoRegisterMessageFilter(null, out oldFilter);
        }

        //
        // IOleMessageFilter functions.
        // Handle incoming thread requests.
        int IOleMessageFilter.HandleInComingCall(int dwCallType,
          System.IntPtr hTaskCaller, int dwTickCount, System.IntPtr
          lpInterfaceInfo)
        {
            //Return the flag SERVERCALL_ISHANDLED.
            return 0;
        }

        // Thread call was rejected, so try again.
        int IOleMessageFilter.RetryRejectedCall(System.IntPtr
          hTaskCallee, int dwTickCount, int dwRejectType)
        {
            if (dwRejectType == 2)
            // flag = SERVERCALL_RETRYLATER.
            {
                // Retry the thread call immediately if return >=0 & 
                // <100.
                return 99;
            }
            // Too busy; cancel call.
            return -1;
        }

        int IOleMessageFilter.MessagePending(System.IntPtr hTaskCallee,
          int dwTickCount, int dwPendingType)
        {
            //Return the flag PENDINGMSG_WAITDEFPROCESS.
            return 2;
        }

        // Implement the IOleMessageFilter interface.
        [DllImport("Ole32.dll")]
        private static extern int
          CoRegisterMessageFilter(IOleMessageFilter newFilter, out 
          IOleMessageFilter oldFilter);
    }

    [ComImport(), Guid("00000016-0000-0000-C000-000000000046"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    interface IOleMessageFilter
    {
        [PreserveSig]
        int HandleInComingCall(
            int dwCallType,
            IntPtr hTaskCaller,
            int dwTickCount,
            IntPtr lpInterfaceInfo);

        [PreserveSig]
        int RetryRejectedCall(
            IntPtr hTaskCallee,
            int dwTickCount,
            int dwRejectType);

        [PreserveSig]
        int MessagePending(
            IntPtr hTaskCallee,
            int dwTickCount,
            int dwPendingType);
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
