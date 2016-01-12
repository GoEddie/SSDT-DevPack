using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using SSDTDevPacl.CodeCoverage.Lib.Ui;

namespace TheAgileSQLClub.SSDTDevPack_VSPackage
{
    /// <summary>
    ///     This class implements the tool window exposed by this package and hosts a user control.
    ///     In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    ///     usually implemented by the package implementer.
    ///     This class derives from the ToolWindowPane class provided from the MPF in order to use its
    ///     implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("978c49d6-2808-4a7a-bf32-274bec0EDD1E")]
    public class CodeCoverageToolWindow : ToolWindowPane
    {
        /// <summary>
        ///     Standard constructor for the tool window.
        /// </summary>
        public CodeCoverageToolWindow() :
            base(null)
        {
            // Set the window title reading it from the resources.
            Caption = "Code Coverage";
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            //   BitmapResourceID = 301;
            //   BitmapIndex = 1;
            Content = new CodeCoverageWindow();
        }

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnClose()
        {
            base.OnClose();
        }
    }
}