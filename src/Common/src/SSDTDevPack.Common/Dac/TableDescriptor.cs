using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.SqlServer.Dac.Extensions.Prototype;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using ColumnType = Microsoft.SqlServer.Dac.Model.ColumnType;

namespace SSDTDevPack.Common.Dac
{
    public class TableDescriptor
    {
        public TableDescriptor(TSqlTable table)
        {
            Columns = BuildColumnDescriptors(table);
            Name = table.Name;
            Constraints = BuildConstraints(table);
            
        }

        private IEnumerable<Constraint> BuildConstraints(TSqlTable table)
        {
            var constraints = new List<Constraint>();

            foreach (var c in table.PrimaryKeyConstraints)
            {
                constraints.Add(new Constraint()
                {
                    Name = c.Name, Type = ConstraintType.PrimaryKey
                });
            }


            return constraints;
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
        public IEnumerable<Constraint> Constraints { get; private set; }
        public ObjectIdentifier Name { get; set; }
        
    }

    public enum ConstraintType
    {
        PrimaryKey,
        ForeignKey,
        Check
    }

    public class Constraint
    {
        public ConstraintType Type;
        public ObjectIdentifier  Name;
    }
}