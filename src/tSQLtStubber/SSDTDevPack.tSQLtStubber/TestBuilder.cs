










using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.ProjectItems;
using SSDTDevPack.Common.SolutionBrowser;

namespace SSDTDevPack.tSQLtStubber
{
    public class TestBuilder : ScriptBuilder
    {
        private readonly string _scripts;
        private readonly Project _sourceProject;

        public TestBuilder(string scripts, Project sourceProject)
        {
            _scripts = scripts;
            _sourceProject = sourceProject;
        }

        public void Go(){

            IList<ParseError> errors;
            var fragment = new TSql120Parser(false).Parse(new StringReader(_scripts), out errors);
            if (fragment == null)
                return;

            var visitor = new ProcedureVisitor();
            fragment.Accept(visitor);

            using (var procedureRepository = new ProcedureRepository(DacpacPath.Get(_sourceProject)))
            using (var functionRepository = new FunctionRepository(DacpacPath.Get(_sourceProject)))
            {
                foreach (var procedure in visitor.Procedures)
                {
                    var browser = new SolutionBrowserForm("test " + procedure.ProcedureReference.Name.BaseIdentifier.Value.UnQuote() + " does something");
                    browser.ShowDialog();

                    var destination = browser.DestinationItem;
                    if (destination == null)
                        continue;

                    if (String.IsNullOrEmpty(DacpacPath.Get(_sourceProject)) && !File.Exists(DacpacPath.Get(_sourceProject)))
                    {
                        MessageBox.Show("Cannot find dacpac for project");
                        return;
                    }
                    
                    var parentProjectItem = destination;
                        
                    var name = browser.GetObjectName();
                    
                    var proc = procedureRepository.FirstOrDefault(p => p.Name.EqualsName(procedure.ProcedureReference.Name));
                    if (proc == null)
                    {
                        MessageBox.Show(string.Format("Cannot find stored procedure {0} in project compiled dacpac", procedure.ProcedureReference.Name));
                        return;
                    }

                    var testBuilder = new ProcedureBuilder(procedure.ProcedureReference.Name.BaseIdentifier.Value.UnQuote(), name, proc);
                    var script = testBuilder.GetScript();

                    CreateNewFile(parentProjectItem, name , script);
                }

                foreach (var procedure in visitor.Functions)
                {
                    var browser = new SolutionBrowserForm("test " + procedure.Name.BaseIdentifier.Value.UnQuote() + " does something");
                    browser.ShowDialog();

                    var destination = browser.DestinationItem;
                    if (destination == null)
                        continue;

                    if (String.IsNullOrEmpty(DacpacPath.Get(_sourceProject)) && !File.Exists(DacpacPath.Get(_sourceProject)))
                    {
                        MessageBox.Show("Cannot find dacpac for project");
                        return;
                    }

                    var parentProjectItem = destination;

                    var name = browser.GetObjectName();

                    var proc = functionRepository.FirstOrDefault(p => p.Name.EqualsName(procedure.Name));
                    if (proc == null)
                    {
                        MessageBox.Show(string.Format("Cannot find stored procedure {0} in project compiled dacpac", procedure.Name));
                        return;
                    }

                    var testBuilder = new ProcedureBuilder(procedure.Name.BaseIdentifier.Value.UnQuote(), name, proc);
                    var script = testBuilder.GetScript();

                    CreateNewFile(parentProjectItem, name, script);
                }
            }
        }

        private static void CreateNewFile(ProjectItem folder, string name, string script)
        {
            var classFolder = folder.ProjectItems.AddFromTemplate("Schema", name.UnQuote() + ".sql");
            var filePath = classFolder.GetStringProperty("FullPath");
            File.WriteAllText(filePath, script);

            classFolder.Open().Visible = true;
        }

    }
}