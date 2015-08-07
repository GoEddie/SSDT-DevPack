using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SSDTDevPack.Common.Dac
{
    public class LiteralConverter
    {
        public Literal BuildLiteral(LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Integer:
                    return new IntegerLiteral();

                case LiteralType.Real:
                    return new RealLiteral();
                case LiteralType.Money:
                    return new MoneyLiteral();
                case LiteralType.Binary:
                    return new BinaryLiteral();
                case LiteralType.String:
                    return new StringLiteral();
                case LiteralType.Null:
                    return new NullLiteral();
                case LiteralType.Default:
                    return new DefaultLiteral();
                case LiteralType.Max:
                    return new MaxLiteral();
                case LiteralType.Odbc:
                    return new OdbcLiteral();
                case LiteralType.Identifier:
                    return new IdentifierLiteral();
                case LiteralType.Numeric:
                    return new NumericLiteral();
            }

            throw new ArgumentException("Unknown literal type");
        }


        public static LiteralType GetLiteralType(ObjectIdentifier name)
        {

            switch (name.GetName())
            {
                case "bigint":
                case "smallint":
                case "tinyint":
                case "int":
                case "bit":
                    return LiteralType.Integer;

                case "numeric":
                case "decimal":
                case "float":
                case "real":
                    return LiteralType.Numeric;


                case "money":
                case "smallmoney":
                    return LiteralType.Money;

                case "date":
                case "datetimeoffset":
                case "datetime2":
                case "smalldatetime":
                case "datetime":
                case "time":
                case "char":
                case "varchar":
                case "text":
                case "nchar":
                case "nvarchar":
                case "ntext":
                case "timestamp":
                case "uniqueidentifier":
                case "sql_variant":
                case "xml":

                    return LiteralType.String;

                case "binary":
                case "varbinary":
                case "image":
                    return LiteralType.Binary;


            }


            return LiteralType.String;
        }

    }
}
