using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using SSDTDevPack.Common.VSPackage;

namespace SSDTDevPack.Common.UserMessages
{
    public static class OutputPane
    {

        public static void WriteMessage(string format, params object[] args)
        {

            IVsOutputWindow outputWindow =  VsServiceProvider.Get(typeof(SVsOutputWindow)) as IVsOutputWindow;

            Guid guidGeneral = VSConstants.OutputWindowPaneGuid.GeneralPane_guid;
            if (outputWindow == null)
                return;
            
            IVsOutputWindowPane pane;
            int hr = outputWindow.CreatePane(guidGeneral, "General", 1, 0);
            hr = outputWindow.GetPane(guidGeneral, out pane);

            if (pane == null)
                return;


            if (!format.EndsWith("\r\n"))
                format = format + "\r\n";

            pane.Activate();
            pane.OutputString(string.Format(format, args));
            
        }

        public static void WriteMessageWithLink(string path, int line, string format, params object[] args)
        {

            IVsOutputWindow outputWindow = VsServiceProvider.Get(typeof(SVsOutputWindow)) as IVsOutputWindow;

            Guid guidGeneral = VSConstants.OutputWindowPaneGuid.GeneralPane_guid;
            if (outputWindow == null)
                return;

            IVsOutputWindowPane pane;
            int hr = outputWindow.CreatePane(guidGeneral, "General", 1, 0);
            hr = outputWindow.GetPane(guidGeneral, out pane);

            if (pane == null)
                return;


            if (!format.EndsWith("\r\n"))
                format = format + "\r\n";

            pane.Activate();
            
            pane.OutputTaskItemString(string.Format(format, args) + "\r\n",
                    VSTASKPRIORITY.TP_NORMAL, VSTASKCATEGORY.CAT_BUILDCOMPILE, "MergeUi", 0, path, (uint)line-1, string.Format(format, args));
                

        }


    }
}
