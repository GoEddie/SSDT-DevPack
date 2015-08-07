using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SSDTDevPack.Merge.MergeDescriptor
{

    interface IInScriptDescriptor
    {
        int ScriptOffset { get; set; }
        int ScriptLength { get; set; }

        string FilePath { get; set; }
    }

    class Merge
    {
        public MergeStatement Statement { get; set; }


    }
}
