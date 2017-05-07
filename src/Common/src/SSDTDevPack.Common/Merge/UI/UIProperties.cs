using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SSDTDevPack.Merge.UI
{
    /// <summary>
    /// If you hide it in plain site, no one will see it - certainly no one would expect you to call a god class God
    /// </summary>
    public static class God
    {
        public static DataTable CurrentMergeData { get; set; }

        public static MergeDescriptor.Merge Merge { get; set; }

        public static Action DataTableChanged { get; set; }

        public static List<MergeDescriptor.Merge> MergesToSave { get; set; }

        static God()
        {
            MergesToSave = new List<MergeDescriptor.Merge>();
        }
    }


}
