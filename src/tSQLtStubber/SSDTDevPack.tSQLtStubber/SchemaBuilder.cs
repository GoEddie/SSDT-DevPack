using System.Collections.Generic;
using System.IO;
using EnvDTE;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.ProjectItems;
using SSDTDevPack.Common.SolutionBrowser;
using SSDTDevPack.tSQLtStubber;

namespace SSDTDevPack.tSQLtStubber
{
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
                var browser = new SolutionBrowserForm(procedure.ProcedureReference.Name.BaseIdentifier.Value.Quote());
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
                    if (item.Name.UnQuote() == name.UnQuote())
                    {
                        CreateNewFile(item, name, script);
                        return;
                    }
                }

                var folder = parentProjectItem.ProjectItems.AddFolder(name.UnQuote());
                CreateNewFile(folder, name, script);
            }

            foreach (var procedure in visitor.Functions)
            {
                var browser = new SolutionBrowserForm(procedure
                    .Name.BaseIdentifier.Value.Quote());
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
                    if (item.Name.UnQuote() == name.UnQuote())
                    {
                        CreateNewFile(item, name, script);
                        return;
                    }
                }

                var folder = parentProjectItem.ProjectItems.AddFolder(name.UnQuote());
                CreateNewFile(folder, name, script);
            }
        }

        private static void CreateNewFile(ProjectItem folder, string name, string script)
        {
            var classFolder = folder.ProjectItems.AddFromTemplate("Schema", name.UnQuote() + ".sql");
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
            createSchema.Owner = new Identifier {Value = "dbo"};

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
            entity.Parameters.Add(ParametersHelper.CreateStoredProcedureParameter("@level0name", schemaName.UnQuote()));

            execExtendedProperty.ExecuteSpecification.ExecutableEntity = entity;

            return GenerateScript(execExtendedProperty);
        }
    }
}