using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.VisualStudio.Text;
using SSDTDevPack.Rewriter;

namespace SSDTDevPack.Clippy.Operations
{
    class InequalityReWriteOperation : ReWriterOperation
    {
        
        public override GlyphDefinition GetDefintions(string fragment, TSqlStatement statement, GlyphDefinition definition, List<QuerySpecification> queries)
        {
            var rewriter = new InEqualityRewriter(fragment);
            var replacements = rewriter.GetReplacements(queries);

            if (replacements == null)
                return definition;

            if (replacements.Count > 0)
            {
                definition.Menu.Add(new MenuDefinition()
                {
                    Caption = "Replace != with <>",
                    Action = () => { },
                    Type = MenuItemType.Header
                    ,
                    Glyph = definition
                });

                var offsettedReplacments = new List<Replacements>();

                //span.Snapshot.CreateTrackingSpan(glyph.Tag.ParentTag.Span.Start+offset, _replacement.OriginalLength, SpanTrackingMode.EdgeNegative);
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
                    Debug.WriteLine("\tLine {2} \"{0}\" into \"{1}\"", replacement.Original, replacement.Replacement, replacement.OriginalFragment.StartLine);
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