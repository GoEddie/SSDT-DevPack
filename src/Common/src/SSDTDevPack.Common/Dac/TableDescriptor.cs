using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.SqlServer.Dac.Extensions.Prototype;
using Microsoft.SqlServer.Dac.Model;

namespace SSDTDevPack.Common.Dac
{
    public class TableDescriptor
    {
        public TableDescriptor(TSqlTable table)
        {
            Columns = BuildColumnDescriptors(table);
            Name = table.Name;
        }

        private List<ColumnDescriptor> BuildColumnDescriptors(TSqlTable table)
        {
            if (!CanDecodeAllColumns(table))
            {
                //throw new InvalidOperationException
                MessageBox.Show("Unable to work out column types for: " + table.Name.GetSchemaObjectName() +
                                                    " \r\nIf it has any user defined types then MergeUi doesn't support that right now");

                return null;
            }

            return table.Columns.Where(column => column.ColumnType == ColumnType.Column).Select(column => new ColumnDescriptor(column)).ToList();
        }

        private bool CanDecodeAllColumns(TSqlTable table)
        {
            try
            {
                foreach (TSqlColumn column in table.Columns.Where(column => column.ColumnType == ColumnType.Column))
                {
                    if (column.ObjectType == UserDefinedType.TypeClass)
                    {
                     
                    }
                    var descriptor = new ColumnDescriptor(column);

                }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public List<ColumnDescriptor> Columns { get; private set; }
        public ObjectIdentifier Name { get; set; }
    }
}