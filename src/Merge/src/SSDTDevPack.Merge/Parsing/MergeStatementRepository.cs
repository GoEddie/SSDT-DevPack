using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.ScriptDom;
using SSDTDevPack.Logging;
using SSDTDevPack.Merge.MergeDescriptor;

namespace SSDTDevPack.Merge.Parsing
{
    public class MergeStatementRepository
    {
        private readonly string _path;
        private readonly TableRepository _tables;

        public MergeStatementRepository(TableRepository tables, string path)
        {
            _tables = tables;
            _path = path;
        }

        public List<MergeDescriptor.Merge> Merges { get; set; }

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
                    Log.WriteInfo(
                        "Error Parsing Merge Statement, Could not find table name ({0}) in the TableRepository",
                        name.BaseIdentifier.Value);
                    continue;
                }

                if (!(mergeStatement.MergeSpecification.TableReference is InlineDerivedTable))
                {
                    Log.WriteInfo("Error Parsing Merge Statement, Could not find InlineDerivedTable");
                    continue;
                }


                var merge = new MergeDescriptor.Merge();
                merge.CustommMerger = mergeStatement;
                merge.Name = name.ToIdentifier();
                
                merge.Data = GetDataFromMerge(mergeStatement, table);
                if (null == merge.Data)
                {
                    continue;   //can't do anything with this
                }
                
                merge.Data.AcceptChanges();
                merge.ScriptDescriptor = new InScriptDescriptor(mergeStatement.StartOffset,
                    mergeStatement.FragmentLength, _path);
                merge.Statement = mergeStatement;
                merge.Table = table;


                bool hasSearchKeys = false;
                var searchCondition = mergeStatement.MergeSpecification.SearchCondition as BooleanComparisonExpression;
                if (null != searchCondition)
                {
                    var col = searchCondition.FirstExpression as ColumnReferenceExpression;
                    if (col != null)
                    {
                        var searchColName = col.MultiPartIdentifier.Identifiers.Last();
                        if (searchColName.Value == "???")
                        {
                            hasSearchKeys = false;
                        }
                        else
                        {
                            hasSearchKeys = true;

                        }

                    }

                }

                merge.Option =
                    new MergeOptions(
                        mergeStatement.MergeSpecification.ActionClauses.Any(p => p.Condition == MergeCondition.Matched),
                        mergeStatement.MergeSpecification.ActionClauses.Any(
                            p => p.Condition == MergeCondition.NotMatchedByTarget),
                        mergeStatement.MergeSpecification.ActionClauses.Any(
                            p => p.Condition == MergeCondition.NotMatchedBySource)
                            ,hasSearchKeys
                            );

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
                            MessageBox.Show("Error we expected to get a literal parsing a merge statement for the " +
                                            table.Name.GetName() + " table but we got a " + col.ToString()
                                            +
                                            "\r\nCheck that you don't have an unquoted string or another impossible type");
                            return null;
                        }

                        dataTableRow[index++] = value.Value;
                    }
                }

                dataTable.Rows.Add(dataTableRow);
            }


            dataTable.RowChanged += (sender, args) =>
            {
                dataTable.ExtendedProperties["Changed"] = true;
            };

            dataTable.TableNewRow += (sender, args) =>
            {
                dataTable.ExtendedProperties["Changed"] = true;
            };

            dataTable.RowDeleting += (sender, args) =>
            {
                dataTable.ExtendedProperties["Changed"] = true;
            };

            return dataTable;
        }
    }
}