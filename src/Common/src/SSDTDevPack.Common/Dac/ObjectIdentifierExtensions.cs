using System;
using System.Linq;
using System.Text;
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

        public static string GetNameFullQuoted(this ObjectIdentifier name)
        {
            var builder = new StringBuilder();
            var first = true;

            foreach (var part in name.Parts.Reverse())
            {
                builder.Append(part.Quote());
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(".");
                }
            }

            return builder.ToString();
        }

        public static string GetNameFullUnQuoted(this ObjectIdentifier name)
        {
            var builder = new StringBuilder();
            var first = true;

            foreach (var part in name.Parts) //.Reverse())
            {
                builder.AppendFormat("{0}.", part.UnQuote());
            }

            var fullName = builder.ToString();
            return fullName.Substring(0, fullName.Length - 1);
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

        public static bool IsQuoted(this string source)
        {
            return source.StartsWith("[");
        }

        public static string CorrectQuote(this string source, QuoteType type)
        {
            switch(type)
            {
                case QuoteType.NotQuoted:
                    return source.UnQuote();
                case QuoteType.SquareBracket:
                    return source.Quote();
                case QuoteType.DoubleQuote:
                    return source.SpeechQuote();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        public static string SpeechQuote(this string source)
        {
            if (!source.StartsWith("\""))
                source = "\"" + source;

            if (!source.EndsWith("\""))
                source = source + "\"";

            return source;
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

    public static class MultiPartIdentifierExtensions
    {
        public static string ToNameString(this MultiPartIdentifier name)
        {
            var builder = new StringBuilder();
            var first = true;

            foreach (var part in name.Identifiers.Reverse())
            {
                builder.Append(part.Value.UnQuote());
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(".");
                }
            }
            return builder.ToString();
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

        public static string ToQuotedString(this SchemaObjectName source)
        {
            if (source.SchemaIdentifier != null)
            {
                return string.Format("{0}.{1}", source.SchemaIdentifier.Value.Quote(), source.BaseIdentifier.Value.Quote());
            }

            return source.BaseIdentifier.Value.Quote();
        }

        public static string ToUnquotedString(this SchemaObjectName source)
        {
            if (source.SchemaIdentifier != null)
            {
                return string.Format("{0}.{1}", source.SchemaIdentifier.Value.UnQuote(), source.BaseIdentifier.Value.UnQuote());
            }

            return source.BaseIdentifier.Value.UnQuote();
        }

        public static bool EqualsObjectIdentifierInsensitive(this ObjectIdentifier destination, SchemaObjectName source)
        {
            if (destination.Parts.Count == 1)
            {
                return destination.Parts[0].UnQuote().ToLowerInvariant() == source.BaseIdentifier.Value.UnQuote().ToLowerInvariant();
            }

            if (destination.Parts.Count == 2)
            {
                return destination.Parts[0].UnQuote().ToLowerInvariant() == source.SchemaIdentifier.Value.UnQuote().ToLowerInvariant() && destination.Parts[1].UnQuote().ToLowerInvariant() == source.BaseIdentifier.Value.UnQuote().ToLowerInvariant();
            }

            if (destination.Parts.Count == 3)
            {
                return destination.Parts[0].UnQuote().ToLowerInvariant() == source.DatabaseIdentifier.Value.UnQuote().ToLowerInvariant() && destination.Parts[1].UnQuote().ToLowerInvariant() == source.SchemaIdentifier.Value.UnQuote().ToLowerInvariant() && destination.Parts[2].UnQuote().ToLowerInvariant() == source.BaseIdentifier.Value.UnQuote().ToLowerInvariant();
            }

            if (destination.Parts.Count == 4)
            {
                return destination.Parts[0].UnQuote().ToLowerInvariant() == source.ServerIdentifier.Value.UnQuote().ToLowerInvariant() && destination.Parts[1].UnQuote().ToLowerInvariant() == source.DatabaseIdentifier.Value.UnQuote().ToLowerInvariant() && destination.Parts[2].UnQuote().ToLowerInvariant() == source.SchemaIdentifier.Value.UnQuote().ToLowerInvariant() && destination.Parts[3].UnQuote().ToLowerInvariant() == source.BaseIdentifier.Value.UnQuote().ToLowerInvariant();
            }

            return false;
        }

        public static bool EqualsObjectIdentifier(this ObjectIdentifier destination, SchemaObjectName source)
        {
            if (destination.Parts.Count == 1)
            {
                return destination.Parts[0].UnQuote() == source.BaseIdentifier.Value.UnQuote();
            }

            if (destination.Parts.Count == 2)
            {
                return destination.Parts[0].UnQuote() == source.SchemaIdentifier.Value.UnQuote() && destination.Parts[1].UnQuote() == source.BaseIdentifier.Value.UnQuote();
            }

            if (destination.Parts.Count == 3)
            {
                return destination.Parts[0].UnQuote() == source.DatabaseIdentifier.Value.UnQuote() && destination.Parts[1].UnQuote() == source.SchemaIdentifier.Value.UnQuote() && destination.Parts[2].UnQuote() == source.BaseIdentifier.Value.UnQuote();
            }

            if (destination.Parts.Count == 4)
            {
                return destination.Parts[0].UnQuote() == source.ServerIdentifier.Value.UnQuote() && destination.Parts[1].UnQuote() == source.DatabaseIdentifier.Value.UnQuote() && destination.Parts[2].UnQuote() == source.SchemaIdentifier.Value.UnQuote() && destination.Parts[3].UnQuote() == source.BaseIdentifier.Value.UnQuote();
            }

            return false;
        }
    }

    public static class TableNameComparer
    {
        public static bool AreSame(ObjectIdentifier id, SchemaObjectName son)
        {
            switch (id.Parts.Count)
            {
                case 4:
                    return id.Parts[0] == son.ServerIdentifier.Value && id.Parts[1] == son.DatabaseIdentifier.Value && id.Parts[2] == son.SchemaIdentifier.Value && id.Parts[3] == son.BaseIdentifier.Value;

                case 3:
                    return id.Parts[0] == son.DatabaseIdentifier.Value && id.Parts[1] == son.SchemaIdentifier.Value && id.Parts[2] == son.BaseIdentifier.Value;

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