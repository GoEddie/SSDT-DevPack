using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace SSDTDevPack.Clippy
{
    public static class ClippySettings
    {
        public static bool Enabled = false;

        public static OleMenuCommand MenuItem;

        public static Action ClippyDisabled;

    }
}
