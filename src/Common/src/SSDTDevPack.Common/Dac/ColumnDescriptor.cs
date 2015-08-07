using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac.Extensions.Prototype;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SSDTDevPack.Common.Dac
{
    public class ColumnDescriptor
    {
        public ColumnDescriptor(TSqlColumn column)
        {
            Name = column.Name;
            DataType = LiteralConverter.GetLiteralType(column.DataType.FirstOrDefault().Name);
            DataLength = column.Length;

            IsIdentity = column.IsIdentity;
            IsKey = column.GetReferencingRelationshipInstances(PrimaryKeyConstraint.Columns).FirstOrDefault() != null;

        }

        public ObjectIdentifier Name { get; set; }
        public LiteralType DataType { get; set; }
        public int DataTypeLength { get; set; }

        public int DataLength { get; set; }
        public bool IsKey { get; set; }
        public bool IsIdentity { get; set; }
        
    }
}
