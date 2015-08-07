using System.Collections.Generic;
using System.Linq;
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
            return table.Columns.Select(column => new ColumnDescriptor(column)).ToList();
        }

        public List<ColumnDescriptor> Columns { get; private set; }
        public ObjectIdentifier Name { get; set; }
    }
}