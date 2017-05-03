using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Merge.MergeDescriptor;

namespace SSDTDevPack.Merge.Parsing
{
    public class MergeStatementFactory
    {
        public MergeDescriptor.Merge Build(TableDescriptor table, string scriptFile, DataTable data = null)
        {
            
            var merge = new MergeDescriptor.Merge();
            merge.Name = table.Name.ToIdentifier();

            if(data == null)
                merge.Data = BuildDataTableDefinition( table);
            else
            {
                merge.Data = data;
            }
            merge.ScriptDescriptor = new InScriptDescriptor(0,0, scriptFile);
            merge.Statement = new MergeStatement();
            merge.Table = table;

           

            merge.Data.AcceptChanges();

            merge.Option = new MergeOptions(true, true, true, merge.Table.Columns.Any(p=>p.IsKey));
            return merge;
        }

        private DataTable BuildDataTableDefinition(TableDescriptor table)
        {
            var dataTable = new DataTable();
            foreach (var col in table.Columns)
            {
                dataTable.Columns.Add(new DataColumn(col.Name.GetName()));
            }

            dataTable.AcceptChanges();

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
