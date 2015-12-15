using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Rewriter;

namespace SSDTDevPack.Clippy.Operations
{
    class OrdinalOrderByReWriteOperation : ReWriterOperation
    {
        public override GlyphDefinition GetDefintions(string fragment, TSqlStatement statement, GlyphDefinition definition, List<QuerySpecification> queries)
        {
            var rewriter = new OrderByOrdinalRewrites();
            var replacements = rewriter.GetReplacements(queries);

            if (replacements == null)
                return definition;

            if (replacements.Count > 0)
            {
                definition.Menu.Add(new MenuDefinition()
                {
                    Caption = "Replace Ordinals in Order By",
                    Action = () => { },
                    Type = MenuItemType.Header
                    ,
                    Glyph = definition
                });

                var offsettedReplacments = new List<Replacements>();
                foreach (var replacement in replacements)
                {
                    var replacement1 = replacement;
                    replacement1.OriginalOffset += statement.StartOffset;
                    offsettedReplacments.Add(replacement1);
                }

                if (replacements.Count > 1)
                {
                    var menu = new MenuDefinition();
                    menu.Operation = new ClippyReplacementOperations(offsettedReplacments);
                    menu.Action = () => PerformAction(menu.Operation, menu.Glyph);

                    menu.Glyph = definition;
                    menu.Caption = GetCaptionForAll(statement);
                    menu.Type = MenuItemType.MenuItem;
                    definition.Menu.Add(menu);
                }


                foreach (var replacement in offsettedReplacments)
                {
                    var menu = new MenuDefinition();
                    menu.Action = () => PerformAction(menu.Operation, menu.Glyph);
                    menu.Glyph = definition;
                    menu.Caption = string.Format("\t\"{0}\" into \"{1}\"", replacement.Original, replacement.Replacement);
                    menu.Type = MenuItemType.MenuItem;
                    menu.Operation = new ClippyReplacementOperation(replacement);
                    definition.Menu.Add(menu);
                }

                definition.GenerateKey();
            }
            return definition;
        }

        public override GlyphDefinition GetDefintions(string fragment, TSqlStatement statement, GlyphDefinition definition, List<DeleteSpecification> queries)
        {
            return definition;
        }

        public override GlyphDefinition GetDefinitions(string fragment, TSqlStatement statement, GlyphDefinition definition, List<TSqlStatement> queries)
        {
            return definition;
        }
    }
}