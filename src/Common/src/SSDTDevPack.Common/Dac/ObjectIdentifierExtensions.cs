using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SSDTDevPack.Common.Dac
{
    public static class ObjectIdentifierExtensions
    {
        public static string GetName(this ObjectIdentifier name)
        {
            return name.Parts.LastOrDefault();
        }

        public static string GetSchema(this ObjectIdentifier name)
        {
            if (name.Parts.Count == 3)
            {
                return name.Parts[1];
            }

            if (name.Parts.Count == 2)
            {
                return name.Parts[0];
            }

            return null;
        }

        public static bool EqualsName(this ObjectIdentifier source, SchemaObjectName target)
        {
            if (target.SchemaIdentifier == null)
                return source.GetName() == target.BaseIdentifier.Value;

            return source.GetSchema() == target.SchemaIdentifier.Value && source.GetName() == target.BaseIdentifier.Value;
        }

    }

    public static class SchemaObjectNameExtensions
    {
        public static ObjectIdentifier ToObjectIdentifier(this SchemaObjectName source)
        {
            return new ObjectIdentifier(source.SchemaIdentifier.Value, source.BaseIdentifier.Value);
        }

        public static Identifier ToIdentifier(this SchemaObjectName source)
        {
            return source.BaseIdentifier;
        }
    }
}
