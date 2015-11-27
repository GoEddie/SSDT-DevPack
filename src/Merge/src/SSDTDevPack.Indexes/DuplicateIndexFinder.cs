using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.Enumerators;
using SSDTDevPack.Common.ScriptDom;
using SSDTDevPack.Common.UserMessages;

namespace SSDTDevPack.Indexes
{
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