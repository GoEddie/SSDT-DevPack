using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.QueryCosts;

namespace SSDTDevPack.Clippy.Operations
{
    internal class QueryCostOperations : ClippyOperationBuilder
    {
        public override GlyphDefinition GetDefintions(string fragment, TSqlStatement statement, GlyphDefinition definition, List<QuerySpecification> queries)
        {
            var documentCoster = DocumentScriptCosters.GetInstance();
            if (documentCoster == null)
                return definition;

            var coster = documentCoster.GetCoster();
            if (null == coster)
                return definition;

            var statements = coster.GetCosts();

            if (statements == null || statements.Count == 0)
                return definition;

            var thisStatement = fragment;

            var costedStatement =
                statements.FirstOrDefault(p => p.Text.IndexOf(thisStatement, StringComparison.OrdinalIgnoreCase) > 0);

            if (costedStatement == null)
                return definition;

            definition.Menu.Add(new MenuDefinition
            {
                Action = null,
                Caption = "Query Cost: " + costedStatement.Cost,
                Type = MenuItemType.Header
            });

            return definition;
        }
    }
}