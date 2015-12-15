using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.Enumerators;

namespace SSDTDevPack.Rewriter
{
    /// <summary>
    /// This is slightly different from the other re-writers in that it does all tables in the batch and the 
    /// calling operation then assigns them to queries to be added to glyphs...
    /// </summary>
    public class TableReferenceRewriter
    {
        private readonly string _script;
        private readonly List<NamedTableReference> _tables;

        public TableReferenceRewriter(string script, List<NamedTableReference> tables)
        {
            _script = script;
            _tables = tables;
        }

        public List<Replacements> GetReplacements(List<TableDescriptor> dacTables)
        {
            var replacements = new List<Replacements>();
            
            foreach (var table in _tables)
            {
                var addedDbo = false;
                var tableName = table.SchemaObject;

                if (table.SchemaObject.SchemaIdentifier == null)
                {
                    tableName.Identifiers.Insert(0,new Identifier() {Value = "dbo"});;
                    addedDbo = true;
                }

                var dacTable = dacTables.FirstOrDefault(p => p.Name.EqualsObjectIdentifierInsensitive(tableName) );

                if (dacTable == null)
                    continue;

                if (!dacTable.Name.EqualsObjectIdentifier(tableName))
                {   //same table different case 
                    
                    var replacement = new Replacements();
                    replacement.OriginalOffset = table.StartOffset;
                    replacement.OriginalFragment = table;
                    replacement.OriginalLength = table.FragmentLength;
                    replacement.Replacement = addedDbo ? dacTable.Name.GetName() : dacTable.Name.GetNameFullUnQuoted();
                    replacements.Add(replacement);
                }

                if (addedDbo)
                    tableName.Identifiers.RemoveAt(0);
            }

             return replacements;
        }


    }
}