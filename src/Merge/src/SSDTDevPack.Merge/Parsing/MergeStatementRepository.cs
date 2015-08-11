using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.ScriptDom;
using SSDTDevPack.Logging;
using SSDTDevPack.Merge.MergeDescriptor;

namespace SSDTDevPack.Merge.Parsing
{
    public class MergeStatementRepository
    {
        private readonly TableRepository _tables;
        private readonly string _path;

        public MergeStatementRepository(TableRepository tables, string path)
        {
            _tables = tables;
            _path = path;
        }

        public List<MergeDescriptor.Merge> Get()
        {
            if (Merges == null)
            {
                Populate();
            }
            return Merges;
        }

        public void Populate()
        {
            Merges = new List<MergeDescriptor.Merge>();
            var statementParser = new MergeStatementParser();

            var parser = new ScriptParser(statementParser, _path);
            parser.Parse();

            foreach (var mergeStatement in statementParser.Merges)
            {
                if (!(mergeStatement.MergeSpecification.Target is NamedTableReference))
                {
                    Log.WriteInfo("Error Parsing Merge Statement, Target is not a NamedTableReference");
                    continue;
                }

                var name = (mergeStatement.MergeSpecification.Target as NamedTableReference).SchemaObject;
                var table = _tables.Get().FirstOrDefault(p => p.Name.EqualsName(name));
                if (table == null)
                {
                    Log.WriteInfo("Error Parsing Merge Statement, Could not find table name ({0}) in the TableRepository", name.BaseIdentifier.Value);
                    continue;
                }

                var merge = new MergeDescriptor.Merge();
                merge.Name = name.ToIdentifier();
                merge.Data = GetDataFromMerge(mergeStatement, table);
                merge.ScriptDescriptor = new InScriptDescriptor(mergeStatement.StartOffset, mergeStatement.FragmentLength, _path);
                merge.Statement = mergeStatement;
                merge.Option =
                    new MergeOptions(
                        mergeStatement.MergeSpecification.ActionClauses.Any(p => p.Condition == MergeCondition.Matched),
                        mergeStatement.MergeSpecification.ActionClauses.Any(p => p.Condition == MergeCondition.NotMatchedByTarget),
                        mergeStatement.MergeSpecification.ActionClauses.Any(p => p.Condition == MergeCondition.NotMatchedBySource));
            
                Merges.Add(merge);
            }

        }

        private DataTable GetDataFromMerge(MergeStatement mergeStatement, TableDescriptor table)
        {
            var dataTable = new DataTable();
            foreach (var col in table.Columns)
            {
                dataTable.Columns.Add(new DataColumn(col.Name.GetName()));
            }

            var inlineTable = mergeStatement.MergeSpecification.TableReference as InlineDerivedTable;
            if (inlineTable == null)
            {
                Log.WriteInfo("Error Parsing Merge Statement, Could not find InlineDerivedTable");
                return null;
            }


            foreach (var row in inlineTable.RowValues)
            {
                var dataTableRow = dataTable.NewRow();
                var index = 0;
                foreach (var col in row.ColumnValues)
                {
                    if (col as NullLiteral != null)
                    {
                        dataTableRow[index++] = DBNull.Value;
                    }
                    else
                    {
                        var value = col as Literal;

                        if (value == null)
                        {
                            Log.WriteInfo("Error Parsing Merge Statement, Could not convert column to Literal: {0}", col);
                            return null;
                        }

                        dataTableRow[index++] = value.Value;
                    }
                }

                dataTable.Rows.Add(dataTableRow);
            }

            return dataTable;
        }

        public List<MergeDescriptor.Merge> Merges { get; set; }

    }
}
