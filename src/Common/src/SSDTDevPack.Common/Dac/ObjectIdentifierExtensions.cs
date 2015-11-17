using System;
using System.Linq;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Merge.MergeDescriptor;

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

        public static string GetSchemaObjectName(this ObjectIdentifier name)
        {
            return string.Format("{0}.{1}", name.GetSchema(), name.GetName());
        }

        public static bool EqualsName(this ObjectIdentifier source, SchemaObjectName target)
        {
            if (target.SchemaIdentifier == null)
                return Quote.Name(source.GetName()) == Quote.Name(target.BaseIdentifier.Value);

            return Quote.Name(source.GetSchema()) == Quote.Name(target.SchemaIdentifier.Value) &&
                   Quote.Name(source.GetName()) == Quote.Name(target.BaseIdentifier.Value);
        }

        public static bool EqualsName(this SchemaObjectName source, SchemaObjectName target)
        {
            return source.BaseIdentifier.Quote() == target.BaseIdentifier.Quote() &&
                   source.SchemaIdentifier.Quote() == target.SchemaIdentifier.Quote();
        }

        public static Identifier ToIdentifier(this ObjectIdentifier source)
        {
            var name = source.GetName();

            return new Identifier
            {
                Value = name.Quote()
            };
        }

        public static SchemaObjectName ToSchemaObjectName(this ObjectIdentifier source)
        {
            var target = new SchemaObjectName();
            target.Identifiers.Add(source.GetSchema().ToScriptDomIdentifier().Quote());
            target.Identifiers.Add(source.GetName().ToScriptDomIdentifier().Quote());

            return target;
        }
    }

    public static class StringExtensions
    {
        public static Identifier ToScriptDomIdentifier(this string source)
        {
            return new Identifier
            {
                Value = source
            };
        }

        public static string Quote(this string source)
        {
            if (!source.StartsWith("["))
                source = "[" + source;

            if (!source.EndsWith("]"))
                source = source + "]";

            return source;
        }

        public static string UnQuote(this string source)
        {
            if (source.StartsWith("["))
                source = source.Substring(1);

            if (source.EndsWith("]"))
                source = source.Substring(0, source.Length - 1);

            return source;
        }
    }

    public static class IdentifierExtensions
    {
        public static Identifier Quote(this Identifier src)
        {
            src.Value = src.Value.Quote();
            return src;
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

    public static class TableNameComparer
    {
        public static bool AreSame(ObjectIdentifier id, SchemaObjectName son)
        {
            switch (id.Parts.Count)
            {
                case 4:
                    return id.Parts[0] == son.ServerIdentifier.Value && id.Parts[1] == son.DatabaseIdentifier.Value
                           && id.Parts[2] == son.SchemaIdentifier.Value && id.Parts[3] == son.BaseIdentifier.Value;

                case 3:
                    return id.Parts[0] == son.DatabaseIdentifier.Value
                           && id.Parts[1] == son.SchemaIdentifier.Value && id.Parts[2] == son.BaseIdentifier.Value;

                case 2:
                    return id.Parts[0] == son.SchemaIdentifier.Value && id.Parts[1] == son.BaseIdentifier.Value;

                case 1:
                    return id.Parts[0] == son.BaseIdentifier.Value;
            }

            return false;
        }
    }

    public static class ObjectNameExtensions
    {
        public static Identifier ToIdentifier(this string src)
        {
            var id = new Identifier {Value = src};
            return id;
        }

        public static SchemaObjectName ToSchemaObjectName(this string src)
        {
            var name = new SchemaObjectName();
            name.Identifiers.Add(src.ToIdentifier());
            return name;
        }

    }

    public class NameConversionException : Exception
    {
        public NameConversionException(string message)
        {
        }
    }
}