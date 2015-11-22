using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SSDTDevPack.Common.UserMessages;
using SSDTDevPack.Common.VSPackage;
using SSDTDevPack.Logging;
using SSDTDevPack.NameConstraints;
using SSDTDevPack.QueryCosts;
using SSDTDevPack.tSQLtStubber;

namespace TheAgileSQLClub.SSDTDevPack_VSPackage
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof (MergeToolWindow))]
    [Guid(GuidList.guidSSDTDevPack_VSPackagePkgString)]
    public sealed class SSDTDevPack_VSPackagePackage : Package, IVsServiceProvider
    {
        public SSDTDevPack_VSPackagePackage()
        {
            VsServiceProvider.Register(this);
        }

        public object GetVsService(Type type)
        {
            return GetService(type);
        }

        private void ShowMergeToolWindow(object sender, EventArgs e)
        {
            var window = FindToolWindow(typeof (MergeToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            var windowFrame = (IVsWindowFrame) window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        protected override void Initialize()
        {
            base.Initialize();

            var mcs = GetService(typeof (IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                var menuCommandID = new CommandID(GuidList.guidSSDTDevPack_VSPackageCmdSet,
                    (int) PkgCmdIDList.SSDTDevPackQuickDeploy);

                var menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);

                var toolwndCommandID = new CommandID(GuidList.guidSSDTDevPack_VSPackageCmdSet,
                    (int) PkgCmdIDList.SSDTDevPackMergeUi);
                var menuToolWin = new MenuCommand(ShowMergeToolWindow, toolwndCommandID);
                mcs.AddCommand(menuToolWin);


                menuCommandID = new CommandID(GuidList.guidSSDTDevPack_VSPackageCmdSet,
                    (int) PkgCmdIDList.SSDTDevPackNameConstraints);
                menuItem = new MenuCommand(NameConstraintsCalled, menuCommandID);
                mcs.AddCommand(menuItem);


                menuCommandID = new CommandID(GuidList.guidSSDTDevPack_VSPackageCmdSet,
                    (int) PkgCmdIDList.SSDTDevPackCreatetSQLtSchema);
                menuItem = new MenuCommand(CreatetSQLtSchema, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidSSDTDevPack_VSPackageCmdSet,
                    (int) PkgCmdIDList.SSDTDevPackCreatetSQLtTestStub);
                menuItem = new MenuCommand(CreatetSQLtTest, menuCommandID);
                mcs.AddCommand(menuItem);


                AddMenuItem(mcs, (int) PkgCmdIDList.SSDTDevPackToggleQueryCosts, ToggleQueryCosts);
                AddMenuItem(mcs, (int)PkgCmdIDList.SSDTDevPackClearQueryCosts, ClearQueryCosts);


                //
            }
        }

        private void ClearQueryCosts(object sender, EventArgs e)
        {
            DocumentScriptCosters.GetInstance().ClearCache();
        }

        private void AddMenuItem(OleMenuCommandService mcs, int cmdId,EventHandler eventHandler )
        {
            CommandID menuCommandID;
            MenuCommand menuItem;
            menuCommandID = new CommandID(GuidList.guidSSDTDevPack_VSPackageCmdSet,
                cmdId);
            menuItem = new MenuCommand(eventHandler, menuCommandID);
            mcs.AddCommand(menuItem);
        }

        private void ToggleQueryCosts(object sender, EventArgs e)
        {
            try
            {
                var dte = (DTE) GetService(typeof (DTE));

                if (dte.ActiveDocument == null)
                {
                    return;
                }

                var doc = dte.ActiveDocument.Object("TextDocument") as TextDocument;
                if (null == doc)
                {
                    return;
                }

                var ep = doc.StartPoint.CreateEditPoint();

                ep.EndOfDocument();

                var length = ep.AbsoluteCharOffset;
                ep.StartOfDocument();

                var originalText = ep.GetText(length);

                DocumentScriptCosters.SetDte(dte);

                var coster = DocumentScriptCosters.GetInstance().GetCoster();

                if (coster.ShowCosts)
                {
                    coster.ShowCosts = false;
                }
                else
                {
                    coster.AddCosts(originalText, dte.ActiveDocument);
                    coster.ShowCosts = true;
                }
            }
            catch (Exception ee)
            {
                OutputPane.WriteMessage("ToggleQueryCosts error: {0}", ee.Message);
                Log.WriteInfo("ToggleQueryCosts error: {0}", ee.Message);
            }
        }

        private void CreatetSQLtTest(object sender, EventArgs e)
        {
            var dte = (DTE) GetService(typeof (DTE));

            if (dte.ActiveDocument == null)
            {
                return;
            }

            var doc = dte.ActiveDocument.Object("TextDocument") as TextDocument;
            if (null == doc)
            {
                return;
            }

            var ep = doc.StartPoint.CreateEditPoint();

            ep.EndOfDocument();

            var length = ep.AbsoluteCharOffset;
            ep.StartOfDocument();

            var originalText = ep.GetText(length);

            var builder = new TestBuilder(originalText, dte.ActiveDocument.ProjectItem.ContainingProject);
            builder.Go();
            //  builder.CreateTests();
        }

        private void CreatetSQLtSchema(object sender, EventArgs e)
        {
            var dte = (DTE) GetService(typeof (DTE));

            if (dte.ActiveDocument == null)
            {
                return;
            }

            var doc = dte.ActiveDocument.Object("TextDocument") as TextDocument;
            if (null == doc)
            {
                return;
            }

            var ep = doc.StartPoint.CreateEditPoint();

            ep.EndOfDocument();

            var length = ep.AbsoluteCharOffset;
            ep.StartOfDocument();

            var originalText = ep.GetText(length);

            var builder = new SchemaBuilder(originalText);
            builder.CreateSchemas();
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            // Show a Message Box to prove we were here
            var uiShell = (IVsUIShell) GetService(typeof (SVsUIShell));
            var clsid = Guid.Empty;
            int result;
            ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                0,
                ref clsid,
                "SSDTDevPack.VSPackage",
                string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", ToString()),
                string.Empty,
                0,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                OLEMSGICON.OLEMSGICON_INFO,
                0, // false
                out result));
        }

        private void NameConstraintsCalled(object sender, EventArgs e)
        {
            var dte = (DTE) GetService(typeof (DTE));
            if (null == dte || dte.ActiveDocument == null)
            {
                return;
            }

            var doc = dte.ActiveDocument.Object("TextDocument") as TextDocument;
            if (null == doc)
            {
                return;
            }

            var ep = doc.StartPoint.CreateEditPoint();

            ep.EndOfDocument();

            var length = ep.AbsoluteCharOffset;
            ep.StartOfDocument();

            var originalText = ep.GetText(length);

            var namer = new ConstraintNamer(originalText);
            var modifiedText = namer.Go();

            if (originalText != modifiedText)
            {
                ep.Delete(length);
                ep.Insert(modifiedText);
            }
        }
    }
}