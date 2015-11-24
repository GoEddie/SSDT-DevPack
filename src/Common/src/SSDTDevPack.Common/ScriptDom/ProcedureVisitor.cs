using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SSDTDevPack.Common.ScriptDom
{
    public class ProcedureVisitor : TSqlFragmentVisitor
    {
        public List<CreateProcedureStatement> Statements = new List<CreateProcedureStatement>(); 

        public override void Visit(CreateProcedureStatement node)
        {
            Statements.Add(node);
        }
    }

    public class FunctionVisitor : TSqlFragmentVisitor
    {
        public List<CreateFunctionStatement> Statements = new List<CreateFunctionStatement>();

        public override void Visit(CreateFunctionStatement node)
        {
            Statements.Add(node);
        }
    }

    public static class ScriptDom
    {
        public static List<CreateProcedureStatement> GetProcedures(string script)
        {
            var parser = new TSql130Parser(false);

            IList<ParseError> errors;
            var s = parser.Parse(new StringReader(script), out errors);

            var visitor = new ProcedureVisitor();

            s.Accept(visitor);

            return visitor.Statements;
        }

        public static List<CreateFunctionStatement> GetFunctions(string script)
        {
            var parser = new TSql130Parser(false);

            IList<ParseError> errors;
            var s = parser.Parse(new StringReader(script), out errors);

            var visitor = new FunctionVisitor();

            s.Accept(visitor);

            return visitor.Statements;
        }

        public static string GenerateTSql(TSqlFragment script)
        {
            var generator = new Sql130ScriptGenerator(Settings.SavedSettings.Get().GeneratorOptions);
            var builder = new StringBuilder();
            generator.GenerateScript(script, new StringWriter(builder));
            return builder.ToString();
        }
    }


}
