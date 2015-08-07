using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;

namespace SSDTDevPack.Merge.MergeDescriptor
{

    class InScriptDescriptor
    {
        public int ScriptOffset { get; set; }
        public int ScriptLength { get; set; }
        public string FilePath { get; set; }
    }

    class Merge
    {
        public MergeStatement Statement { get; set; }

        public InScriptDescriptor ScriptDescriptor { get; set; }

        public DataTable Date { get; set; }

        public TableDescriptor Table;

        public Identifier Name;

        public MergeOptions Option { get; set; }
    }

    public class MergeOptions
    {
        public bool HasUpdate { get; set; }
        public bool HasInsert { get; set; }
        public bool HasDelete { get; set; }
    }
}
