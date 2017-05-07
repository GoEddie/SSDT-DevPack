using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.ProjectItems;
using SSDTDevPack.Common.ScriptDom;
using SSDTDevPack.Common.SolutionBrowser;

namespace SSDTDevPack.Extraction
{
    public class CodeExtractor
    {
        private readonly string _code;
        
        public CodeExtractor(string code)
        {
            _code = code;
        }

        public string ExtractIntoFunction()
        {
            var browser = new SolutionBrowserForm("");
            browser.ShowDialog();

            var destination = browser.DestinationItem;
            if (destination == null)
                return null;
            
            var name = browser.GetObjectName();           
            
            var function = new CreateFunctionStatement();
            var returnSelect = (function.ReturnType = new SelectFunctionReturnType()) as SelectFunctionReturnType;
            returnSelect.SelectStatement = GetSelectStatementForQuery();
            function.Name = name.ToSchemaObjectName();
            
            var classFolder = destination.ProjectItems.AddFromTemplate("Procedure", name.UnQuote() + ".sql");
            var filePath = classFolder.GetStringProperty("FullPath");
            
            File.WriteAllText(filePath, ScriptDom.GenerateTSql(function));

            classFolder.Open().Visible = true;
            
            return GetCallingCode(function);

       }

        private string GetCallingCode(CreateFunctionStatement function)
        {
            var callingSelect = new SelectStatement();
            var spec = (callingSelect.QueryExpression = new QuerySpecification()) as QuerySpecification;
            spec.FromClause = new FromClause();
            spec.FromClause.TableReferences.Add(new SchemaObjectFunctionTableReference()
            {
                SchemaObject = function.Name
            });

            spec.SelectElements.Add(new SelectStarExpression());

            return ScriptDom.GenerateTSql(spec);
        }

        private SelectStatement GetSelectStatementForQuery()
        {
            return ScriptDom.GetSelects(_code).FirstOrDefault();
        }

       
    }


    
        
}
