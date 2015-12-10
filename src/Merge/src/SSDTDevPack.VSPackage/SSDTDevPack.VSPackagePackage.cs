using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SSDTDevPack.Clippy;
using SSDTDevPack.Common.ScriptDom;
using SSDTDevPack.Common.Settings;
using SSDTDevPack.Common.UserMessages;
using SSDTDevPack.Common.VSPackage;
using SSDTDevPack.Extraction;
using SSDTDevPack.Formatting;
using SSDTDevPack.Rewriter;
using SSDTDevPack.Logging;
using SSDTDevPack.NameConstraints;
using SSDTDevPack.QueryCosts;
using SSDTDevPack.QuickDeploy;
using SSDTDevPack.tSQLtStubber;

namespace TheAgileSQLClub.SSDTDevPack_VSPackage
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof (MergeToolWindow))]
    [Guid(GuidList.guidSSDTDevPack_VSPackagePkgString)]
    [ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]
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
            
                var toolwndCommandID = new CommandID(GuidList.guidSSDTDevPack_VSPackageCmdSet,
                    (int) PkgCmdIDList.SSDTDevPackMergeUi);
                var menuToolWin = new MenuCommand(ShowMergeToolWindow, toolwndCommandID);
                mcs.AddCommand(menuToolWin);


                var menuCommandID = new CommandID(GuidList.guidSSDTDevPack_VSPackageCmdSet,
                    (int) PkgCmdIDList.SSDTDevPackNameConstraints);
                var menuItem = new MenuCommand(NameConstraintsCalled, menuCommandID);
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

                AddMenuItem(mcs, (int)PkgCmdIDList.SSDTDevPackQuickDeploy, QuickDeploy);

                AddMenuItem(mcs, (int)PkgCmdIDList.SSDTDevPackLowerCase, LowerCase);
                AddMenuItem(mcs, (int)PkgCmdIDList.SSDTDevPackUpperCase, UpperCase);

                AddMenuItem(mcs, (int)PkgCmdIDList.SSDTDevPackExtractToTvf, ExtractToTvf);

                AddMenuItem(mcs, (int)PkgCmdIDList.SSDTDevPackFindDuplicateIndexes, FindDuplicateIndexes);
                AddMenuItem(mcs, (int)PkgCmdIDList.SSDTNonSargableRewrites, RewriteNonSargableIsNull);
                AddCheckableMenuItem(mcs, (int)PkgCmdIDList.SSDTTSqlClippy, EnableClippy);

            }
        }


        private void EnableClippy(object sender, EventArgs e)
        {
            ClippySettings.Enabled = !ClippySettings.Enabled;
        }

        private void RewriteNonSargableIsNull(object sender, EventArgs e)
        {
            try
            {
                var oldDoc = GetCurrentDocumentText();
                var newDoc = oldDoc;

                var rewriter = new NonSargableRewrites(oldDoc);
                var queries = ScriptDom.GetQuerySpecifications(oldDoc);
                foreach (var rep in rewriter.GetReplacements(queries))
                {
                    newDoc = newDoc.Replace(rep.Original, rep.Replacement);
                    OutputPane.WriteMessage("Non-Sargable IsNull re-written from \r\n\"{0}\" \r\nto\r\n\"{1}\"\r\n", rep.Original, rep.Replacement);
                }

                if(oldDoc != newDoc)
                    SetCurrentDocumentText(newDoc);

            }
            catch (Exception ex)
            {
                OutputPane.WriteMessage("Error re-writing non sargable isnulls {0}", ex.Message);
            }

        }

        private void FindDuplicateIndexes(object sender, EventArgs e)
        {
            try
            {
                var finder = new DuplicateIndexFinder();
                finder.ShowDuplicateIndexes();
            }
            catch (Exception ex)
            {
                OutputPane.WriteMessage("Error finding duplicatevindexes: {0}", ex.Message);
            }
        }


        private void ExtractToTvf(object sender, EventArgs e)
        {
            try
            {
                var dte = (DTE) GetService(typeof (DTE));

                if (dte.ActiveDocument == null)
                {
                    return;
                }

                var doc = dte.ActiveDocument;

                var text = GetCurrentText();
                if (String.IsNullOrEmpty(text))
                    return;

                var newText = new CodeExtractor(text).ExtractIntoFunction();

                if (text != newText && !String.IsNullOrEmpty(newText))
                {
                    doc.Activate();
                    SetCurrentText(newText);
                    OutputPane.WriteMessage("Code extracted into an inline table valued function");
                }
            }
            catch (Exception ex)
            {
                OutputPane.WriteMessage("Error extracting code into a TVF: {0}", ex.Message);
            }
        }

        //NOT ALL KEYWORDS ARE done LIKE "RETURN"  or datatypes
        private void UpperCase(object sender, EventArgs e)
        {
            try
            {
                var text = GetCurrentDocumentText();
                if (String.IsNullOrEmpty(text))
                    return;

                var newText = KeywordCaser.KeywordsToUpper(text);

                if (text != newText)
                {
                    SetCurrentDocumentText(newText);
                    OutputPane.WriteMessage("Changed keywords to UPPER CASE");
                }
            }
            catch (Exception ex)
            {
                OutputPane.WriteMessage("Exception changing keywords to UPPER CASE, error: {0}", ex.Message);
                
            }
        }
        
        private void LowerCase(object sender, EventArgs e)
        {
            try
            {
                var text = GetCurrentDocumentText();
                if (String.IsNullOrEmpty(text))
                    return;

                var newText = KeywordCaser.KeywordsToLower(text);

                if (text != newText)
                {
                    SetCurrentDocumentText(newText);
                    OutputPane.WriteMessage("Changed keywords to lower case");
                }
            }
            catch (Exception ex)
            {
                OutputPane.WriteMessage("Exception changing keywords to UPPER CASE, error: {0}", ex.Message);
            }
        }

        private void QuickDeploy(object sender, EventArgs e)
        {
            try
            {
                QuickDeployer.DeployFile(GetCurrentDocumentText());
            }
            catch (Exception ex)
            {
                OutputPane.WriteMessage("QuickDeploy error: {0}", ex.Message);
                Log.WriteInfo("QuickDeploy error: {0}", ex.Message);
            }
            
        }

        private void SetCurrentDocumentText(string newText)
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
            ep.Delete(length);
         
            ep.Insert(newText);
        }


        private void SetCurrentText(string newText)
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

            doc.Selection.Text = newText;
        }

        private string GetCurrentDocumentText()
        {

            var dte = (DTE) GetService(typeof (DTE));

            if (dte.ActiveDocument == null)
            {
                return null;
            }

            var doc = dte.ActiveDocument.Object("TextDocument") as TextDocument;
            if (null == doc)
            {
                return null;
            }

            var ep = doc.StartPoint.CreateEditPoint();
            ep.EndOfDocument();

            var length = ep.AbsoluteCharOffset;
            ep.StartOfDocument();
            return ep.GetText(length);
        }

        private string GetCurrentText()
        {
            var dte = (DTE)GetService(typeof(DTE));

            if (dte.ActiveDocument == null)
            {
                return null;
            }

            var doc = dte.ActiveDocument.Object("TextDocument") as TextDocument;
            if (null == doc)
            {
                return null;
            }

            return doc.Selection.Text;
        }

        private void ClearQueryCosts(object sender, EventArgs e)
        {
            DocumentScriptCosters.GetInstance().ClearCache();
        }

        private void AddMenuItem(OleMenuCommandService mcs, int cmdId,EventHandler eventHandler )
        {
            CommandID menuCommandID;
            MenuCommand menuItem;
            menuCommandID = new CommandID(GuidList.guidSSDTDevPack_VSPackageCmdSet, cmdId);
            menuItem = new MenuCommand(eventHandler, menuCommandID);
            mcs.AddCommand(menuItem);

            var a = new OleMenuCommand(eventHandler, menuCommandID);
            a.Checked = false;
        }
        private void AddCheckableMenuItem(OleMenuCommandService mcs, int cmdId, EventHandler eventHandler)
        {
            var menuCommandID = new CommandID(GuidList.guidSSDTDevPack_VSPackageCmdSet, cmdId);
            
            ClippySettings.MenuItem = new OleMenuCommand(eventHandler, menuCommandID);

            ClippySettings.MenuItem.Checked = false;
            ClippySettings.MenuItem.BeforeQueryStatus += a_BeforeQueryStatus;
            mcs.AddCommand(ClippySettings.MenuItem);
        }

        void a_BeforeQueryStatus(object sender, EventArgs e)
        {
            ClippySettings.MenuItem.Checked = ClippySettings.Enabled;
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
                if (coster == null)
                    return;

                if (coster.ShowCosts)
                {
                    coster.ShowCosts = false;
                }
                else
                {
                    coster.ShowCosts = true;
                    coster.AddCosts(originalText, dte.ActiveDocument);
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

                var builder = new TestBuilder(originalText, dte.ActiveDocument.ProjectItem.ContainingProject);
                builder.Go();
                //  builder.CreateTests();
            }
            catch (Exception ex)
            {
                OutputPane.WriteMessage("Exception creating tSQLt tests, error: {0}", ex.Message);
            }
        }

        private void CreatetSQLtSchema(object sender, EventArgs e)
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

                var builder = new SchemaBuilder(originalText);
                builder.CreateSchemas();
            }
            catch (Exception ex)
            {
                OutputPane.WriteMessage("Exception creating tSQLt schema, error: {0}", ex.Message);
            }
        }

        
        private void NameConstraintsCalled(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                OutputPane.WriteMessage("Exception naming constraints, error: {0}", ex.Message);
            }
        }
    }
}