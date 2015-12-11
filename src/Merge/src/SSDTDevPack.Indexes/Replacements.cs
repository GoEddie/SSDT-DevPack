using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SSDTDevPack.Rewriter
{
    public struct Replacements
    {
        public string Original;
        public TSqlFragment OriginalFragment;
        public int OriginalLength;
        public int OriginalOffset;
        public string Replacement;
    }
}