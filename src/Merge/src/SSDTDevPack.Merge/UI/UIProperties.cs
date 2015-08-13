using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SSDTDevPack.Merge.UI
{
    public static class God
    {
        public static DataTable CurrentMergeData { get; set; }

        public static Action DataTableChanged { get; set; }
    }


}
