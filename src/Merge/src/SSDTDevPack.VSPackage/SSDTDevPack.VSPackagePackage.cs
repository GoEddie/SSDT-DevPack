using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SSDTDevPack.Common.SolutionBrowser;
using SSDTDevPack.Common.VSPackage;
using SSDTDevPack.NameConstraints;
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
                    (int)PkgCmdIDList.SSDTDevPackCreatetSQLtSchema);
                menuItem = new MenuCommand(CreatetSQLtSchema, menuCommandID);
                mcs.AddCommand(menuItem);

            }
        }

        private void CreatetSQLtSchema(object sender, EventArgs e)
        {
            var dte = (DTE)GetService(typeof(DTE));
            
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