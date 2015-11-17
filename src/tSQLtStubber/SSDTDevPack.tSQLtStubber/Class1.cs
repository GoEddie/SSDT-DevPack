using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.ProjectItems;
using SSDTDevPack.Common.Settings;
using SSDTDevPack.Common.SolutionBrowser;

namespace SSDTDevPack.tSQLtStubber
{
    public class ScriptBuilder
    {
        protected string GenerateScript(TSqlFragment fragment)
        {
            string script;
            var generator = new Sql120ScriptGenerator(SavedSettings.Get().GeneratorOptions);
            generator.GenerateScript(fragment, out script);
            return script;
        }

        protected IList<TSqlParserToken> GetTokens(TSqlFragment fragment)
        {
            var generator = new Sql120ScriptGenerator(SavedSettings.Get().GeneratorOptions);
            return generator.GenerateTokens(fragment);
        }
    }

    public struct NewFileDefinition
    {
        public string Path;
        public string Contents;
    }

    public class ProcedureVisitor : TSqlFragmentVisitor
    {
        public readonly List<CreateProcedureStatement> Procedures = new List<CreateProcedureStatement>();

        public override void Visit(CreateProcedureStatement node)
        {
            Procedures.Add(node);
        }
    }

    public class SchemaBuilder : ScriptBuilder
    {
        private readonly string _scripts;
        
        public SchemaBuilder(string scripts)
        {
            _scripts = scripts;
            
        }

        public void CreateSchemas()
        {
            IList<ParseError> errors;
            var fragment = new TSql120Parser(false).Parse(new StringReader(_scripts), out errors);
            if (fragment == null)
                return;

            var visitor = new ProcedureVisitor();
            fragment.Accept(visitor);

            foreach (var procedure in visitor.Procedures)
            {
                var browser = new SolutionBrowserForm(procedure.ProcedureReference.Name.BaseIdentifier.Value.ToString().Quote());
                browser.ShowDialog();

                var destination = browser.DestinationItem;
                if (destination == null)
                    continue;

                var parentProjectItem = destination;

                var name = browser.GetObjectName();
                var script = GetScript(name);

                for (var i = 1; i <= parentProjectItem.ProjectItems.Count; i++)
                {
                    var item = parentProjectItem.ProjectItems.Item(i);
                    if (item.Name == name)
                    {
                       CreateNewFile(item, name, script);
                       return;
                    }
                }

                var folder = parentProjectItem.ProjectItems.AddFolder(name);
                CreateNewFile(folder, name, script);
            }

        }

        private static void CreateNewFile(ProjectItem folder, string name, string script)
        {
            ProjectItem classFolder = folder.ProjectItems.AddFromTemplate("Schema", name + ".sql");
            var filePath = classFolder.GetStringProperty("FullPath");
            File.WriteAllText(filePath, script);
        }


        private string GetScript(string schemaName)
        {
            return string.Format("{0}\r\nGO\r\n{1}", GetSchema(schemaName), GetExtendedProperty(schemaName));
        }

        private string GetSchema(string schemaName)
        {
            var createSchema = new CreateSchemaStatement();
            createSchema.Name = new Identifier {Value = schemaName.UnQuote(), QuoteType = QuoteType.SquareBracket};
            createSchema.Owner = new Identifier { Value = "dbo" };

            return GenerateScript(createSchema);
        }

        private string GetExtendedProperty(string schemaName)
        {
            var execExtendedProperty = new ExecuteStatement();
            execExtendedProperty.ExecuteSpecification = new ExecuteSpecification();

            var name = new ChildObjectName();
            name.Identifiers.Add(new Identifier {Value = _scripts});

            var procedureReference = new ProcedureReference();
            procedureReference.Name = "sp_addextendedproperty".ToSchemaObjectName();

            var entity = new ExecutableProcedureReference();
            entity.ProcedureReference = new ProcedureReferenceName();
            entity.ProcedureReference.ProcedureReference = procedureReference;


            entity.Parameters.Add(ParametersHelper.CreateStoredProcedureParameter("@name", "tSQLt.TestClass"));
            entity.Parameters.Add(ParametersHelper.CreateStoredProcedureParameter("@value", 1));
            entity.Parameters.Add(ParametersHelper.CreateStoredProcedureParameter("@level0type", "SCHEMA"));
            entity.Parameters.Add(ParametersHelper.CreateStoredProcedureParameter("@level0name", schemaName));

            execExtendedProperty.ExecuteSpecification.ExecutableEntity = entity;

            return GenerateScript(execExtendedProperty);
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

        public static SchemaObjectName ToSchemaObjectName(this ObjectIdentifier src)
        {
            var name = new SchemaObjectName();

            var items = src.Parts.Count;

            if (items == 0)
            {
                throw new NameConversionException("Didn't find any name parts on ObjectIdentifier");
            }

            if (items == 1)
            {
                name.Identifiers.Add(src.Parts[0].ToIdentifier());
                return name;
            }

            if (items == 2)
            {
                name.Identifiers.Add(src.Parts[0].ToIdentifier());
                name.Identifiers.Add(src.Parts[1].ToIdentifier());
                return name;
            }

            name.Identifiers.Add(src.Parts[items - 2].ToIdentifier());
            name.Identifiers.Add(src.Parts[items - 1].ToIdentifier());
            return name;
        }
    }

    public class NameConversionException : Exception
    {
        public NameConversionException(string message)
        {
        }
    }

    public class Parameter
    {
        public string Name;
        public SqlDataType Type;

        public Parameter(string name, SqlDataType type)
        {
            Name = name;
            Type = type;
        }
    }

    public class ParametersHelper
    {
        public static ExecuteParameter CreateStoredProcedureParameter(string name, string value)
        {
            return new ExecuteParameter
            {
                Variable = new VariableReference
                {
                    Name = name
                },
                ParameterValue = new StringLiteral {Value = value}
            };
        }

        public static ExecuteParameter CreateStoredProcedureParameter(string name, int value)
        {
            return new ExecuteParameter
            {
                Variable = new VariableReference
                {
                    Name = name
                },
                ParameterValue = new IntegerLiteral {Value = value.ToString()}
            };
        }

        public static ExecuteParameter CreateStoredProcedureVariableParameter(string name)
        {
            return new ExecuteParameter
            {
                ParameterValue = new VariableReference
                {
                    Name = name
                }
            };
        }

        public static ExecuteParameter CreateStoredProcedureParameter(string value)
        {
            return new ExecuteParameter
            {
                ParameterValue = new StringLiteral
                {
                    Value = value
                }
            };
        }
    }
}