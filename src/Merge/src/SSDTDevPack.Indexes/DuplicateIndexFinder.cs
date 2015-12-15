using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.Enumerators;
using SSDTDevPack.Common.ScriptDom;
using SSDTDevPack.Common.UserMessages;

namespace SSDTDevPack.Rewriter
{
    public class CorrectCaseTableFinder
    {
        public void CorrectCaseAllTableNames()
        {
            var statements = new StatementEnumerator().GetStatements();

            var dacTables = new List<TableDescriptor>();

            foreach (var project in new ProjectEnumerator().Get(ProjectType.SSDT))
            {
                try
                {
                    var path = DacpacPath.Get(project);
                    dacTables.AddRange(new TableRepository(path).Get());
                }
                catch (Exception ex)
                {
                    OutputPane.WriteMessage("Error getting list of tables in project: {0}, error: {1}", project.Name, ex.Message);
                }
            }

            var alreadyChanged = new Dictionary<string, object>();
            

            foreach (var statement in statements.OrderByDescending(p=>p.Statement.StartOffset))
            {
                var scriptTables = ScriptDom.GetTableList(statement.Statement).Where(p => p is NamedTableReference).Cast<NamedTableReference>().ToList();
                
                var rewriter = new TableReferenceRewriter(statement.Script, scriptTables);
                var replacements = rewriter.GetReplacements(dacTables);

                
                var approvedReplacements = new List<Replacements>();

                foreach (var replacement in replacements)
                {
                    var key = string.Format("{0}:{1}:{2}", statement.FileName, replacement.OriginalOffset, replacement.OriginalLength);

                    if (alreadyChanged.ContainsKey(key))
                        continue;

                    approvedReplacements.Add(replacement);
                    alreadyChanged[key] = null;
                }

                if (approvedReplacements.Count > 0)
                {
                    var script = File.ReadAllText(statement.FileName);
                    OutputPane.WriteMessage("File: {0}", statement.FileName);
                    foreach (var replacement in approvedReplacements.OrderByDescending(p => p.OriginalOffset))
                    {
                        var from = script.Substring(replacement.OriginalOffset, replacement.OriginalLength);
                       
                        var to = replacement.Replacement;
                        OutputPane.WriteMessageWithLink(statement.FileName, statement.Line, "\tChanging case of {0} to {1}", from, to);
                        script = script.Substring(0, replacement.OriginalOffset) + to + script.Substring(replacement.OriginalOffset + replacement.OriginalLength);
                        
                    }

                    File.WriteAllText(statement.FileName, script);
                }
            }
        }   
    }

    public class DuplicateIndexFinder
    {
        public void ShowDuplicateIndexes()
        {
            var statements = new StatementEnumerator().GetIndexes();

            var indexes = new Dictionary<string, List<CodeStatement<CreateIndexStatement>>>();

            foreach (var statement in statements)
            {
                var key = BuildKey(statement.Statement);

                if (indexes.ContainsKey(key))
                {
                    indexes[key].Add(statement);
                }
                else
                {
                    indexes[key] = new List<CodeStatement<CreateIndexStatement>> {statement};
                }
            }
            
            var dups = indexes.Where(p => p.Value.Count > 1);
            foreach (var d in dups)
            {
                OutputPane.WriteMessage("Duplicate Indexes Found: ");

                foreach (var statement in d.Value)
                {
                    OutputPane.WriteMessageWithLink(statement.FileName, statement.Line, "{0}",
                        ScriptDom.GenerateTSql(statement.Statement));
                }
            }

            if (dups == null || !dups.Any())
            {
                OutputPane.WriteMessage("No Duplicate Indexes Found.");
            }
        }

        private string BuildKey(CreateIndexStatement index)
        {
            var key = new StringBuilder();

            if (index.OnName.Count == 1)
            {
                key.Append("dbo");
            }
            else
            {
                key.Append(index.OnName.SchemaIdentifier.Value.UnQuote().ToLower());
            }

            key.Append(index.OnName.BaseIdentifier.Value.UnQuote().ToLower());


            foreach (var i in index.Columns)
            {
                key.AppendFormat("%$%{0}",
                    i.Column.MultiPartIdentifier.Identifiers.LastOrDefault().Value.UnQuote().ToLower());
            }

            foreach (var i in index.IncludeColumns)
            {
                key.AppendFormat("£&^%%I!{0}",
                    i.MultiPartIdentifier.Identifiers.LastOrDefault().Value.UnQuote().ToLower());
            }

            return key.ToString();
        }
    }
}