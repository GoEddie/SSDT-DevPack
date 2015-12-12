using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SSDTDevPack.Clippy.Operations
{
    public abstract class ClippyOperationBuilder
    {
        public abstract GlyphDefinition GetDefintions(string fragment, TSqlStatement statement, GlyphDefinition definition, List<QuerySpecification> queries);
        public abstract GlyphDefinition GetDefintions(string fragment, TSqlStatement statement, GlyphDefinition definition, List<DeleteSpecification> queries);
        protected string GetCaptionForAll(TSqlStatement statement)
        {
            if (statement is SelectStatement)
                return "All on Statement";

            if (statement.GetType().Name.StartsWith("Create"))
                return "All in Module";

            return "All";
        }
    }
}