using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.Enumerators;
using SSDTDevPack.Common.ScriptDom;
using SSDTDevPack.Rewriter;

namespace SSDTDevPack.Clippy.Operations
{
    internal class TableNameCorrectCaser : ReWriterOperation
    {
        public override GlyphDefinition GetDefintions(string fragment, TSqlStatement statement, GlyphDefinition definition, List<QuerySpecification> queries)
        {
            return definition;
        }

        public override GlyphDefinition GetDefintions(string fragment, TSqlStatement statement, GlyphDefinition definition, List<DeleteSpecification> queries)
        {
            return definition;
        }

        public override GlyphDefinition GetDefinitions(string fragment, TSqlStatement statement, GlyphDefinition definition, List<TSqlStatement> queries)
        {
            var scriptTables = ScriptDom.GetTableList(statement).Where(p => p is NamedTableReference).Cast<NamedTableReference>().ToList();
            
            var dacTables = GetDacTables();

            var rewriter = new TableReferenceRewriter(fragment, scriptTables);
            var replacements = rewriter.GetReplacements(dacTables);

            if (replacements.Count > 0)
            {
                definition.Menu.Add(new MenuDefinition()
                {
                    Caption = "Correct Case Table Identifiers",
                    Action = () => { },
                    Type = MenuItemType.Header
                    ,
                    Glyph = definition
                });

                var offsettedReplacments = new List<Replacements>();
                foreach (var replacement in replacements)
                {
                    var replacement1 = replacement;
                    
                    replacement1.Original = fragment.Substring(replacement1.OriginalOffset-statement.StartOffset, replacement1.OriginalLength);
                  
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

            }

            return definition;
        }


        private static readonly List<TableDescriptor> _tableCache = new List<TableDescriptor>();
        private static readonly Dictionary<string, DateTime> _buildTimes = new Dictionary<string, DateTime>(); 
        private static List<TableDescriptor> GetDacTables()
        {
            bool rebuildCache = false;

            foreach (var project in new ProjectEnumerator().Get(ProjectType.SSDT))
            {
                try
                {
                    var path = DacpacPath.Get(project);
                    
                    if (_buildTimes.ContainsKey(path))
                    {
                        var lastTime = _buildTimes[path];
                        var lastWriteTime = File.GetLastWriteTimeUtc(path);

                        if (lastTime < lastWriteTime)
                        {
                            rebuildCache = true;
                        }
                    }
                    else
                    {
                        rebuildCache = true;
                    }
                    
                }
                catch (Exception)
                {
                }
            }

            if (!rebuildCache)
                return _tableCache;

            _tableCache.Clear();

            foreach (var project in new ProjectEnumerator().Get(ProjectType.SSDT))
            {
                try
                {
                    var path = DacpacPath.Get(project);
                    var lastWriteTime = File.GetLastWriteTimeUtc(path);

                    

                    _tableCache.AddRange(new TableRepository(path).Get());
                    _buildTimes[path] = lastWriteTime;
                }
                catch (Exception)
                {
                }
            }
            
            return _tableCache;
        }
    }
}