using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SSDTDevPack.Common.Dac
{
    public class ColumnDescriptor
    {
        public ColumnDescriptor(Identifier name, LiteralType dataType, int dataLength, bool isKey, bool isIdentity, bool isMax)
        {
            Name = name;
            DataType = dataType;
            DataLength = dataLength;
            IsKey = isKey;
            IsIdentity = isIdentity;
            IsMax = isMax;
        }

        public Identifier Name { get; set; }
        public LiteralType DataType { get; set; }
        public int DataTypeLength { get; set; }

        public int DataLength { get; set; }
        public bool IsKey { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsMax { get; set; }
    }
}
