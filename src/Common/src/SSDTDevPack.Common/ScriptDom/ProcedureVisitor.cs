using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Settings;

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

    public class SelectVisitor : TSqlFragmentVisitor
    {
        public List<SelectStatement> Statements = new List<SelectStatement>();

        public override void ExplicitVisit(SelectStatement node)
        {
            Statements.Add(node);

            base.ExplicitVisit(node);
        }
    }

    public class IndexVisitor : TSqlFragmentVisitor
    {
        public List<CreateIndexStatement> Statements = new List<CreateIndexStatement>();

        public override void Visit(CreateIndexStatement node)
        {
            Statements.Add(node);
        }
    }

    public class StatementVisitor : TSqlFragmentVisitor
    {
        public List<TSqlStatement> Statements = new List<TSqlStatement>();

        public override void Visit(TSqlStatement node)
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

        public static List<SelectStatement> GetSelects(string script)
        {
            var parser = new TSql130Parser(false);

            IList<ParseError> errors;
            var s = parser.Parse(new StringReader(script), out errors);

            var visitor = new SelectVisitor();

            s.Accept(visitor);

            return visitor.Statements;
        }

        public static List<CreateIndexStatement> GetCreateIndex(string script)
        {
            var parser = new TSql130Parser(false);

            IList<ParseError> errors;
            var s = parser.Parse(new StringReader(script), out errors);

            var visitor = new IndexVisitor();

            s.Accept(visitor);

            return visitor.Statements;
        }

        public static List<TSqlStatement> GetStatements(string script, out IList<ParseError> errors)
        {
            var parser = new TSql130Parser(false);

            //IList<ParseError> errors;
            var s = parser.Parse(new StringReader(script), out errors);

            var visitor = new StatementVisitor();

            s.Accept(visitor);

            return visitor.Statements;
        }

        public static string GenerateTSql(TSqlFragment script)
        {
            var generator = new Sql130ScriptGenerator(SavedSettings.Get().GeneratorOptions);
            var builder = new StringBuilder();
            generator.GenerateScript(script, new StringWriter(builder));
            return builder.ToString();
        }

        public static TSqlFragment GetFragment(string script)
        {
            var parser = new TSql130Parser(false);
            IList<ParseError> errors;
            return parser.Parse(new StringReader(script), out errors);
        }

        public static List<QuerySpecification> GetQuerySpecifications(string script)
        {
            IList<ParseError> errors;
            var statements = GetStatements(script, out errors);
            var querySpecifications = new List<QuerySpecification>();


            foreach (var s in statements)
            {
                querySpecifications.AddRange(FlattenTreeGetQuerySpecifications(s));
                Console.WriteLine(s);
                //if (s is InsertStatement)
                //{
                //    if((s as InsertStatement).InsertSpecification.InsertSource is SelectInsertSource)
                //    {
                //        querySpecifications.Add(((s as InsertStatement).InsertSpecification.InsertSource as SelectInsertSource).Select as QuerySpecification);
                //    }
                //}

                //if(s is SelectStatement)
                //    querySpecifications.Add((s as SelectStatement).QueryExpression as QuerySpecification);
            }

            return querySpecifications.Where(p => p != null).ToList();
        }

        public static List<QuerySpecification> FlattenTreeGetQuerySpecifications(TSqlStatement statement)
        {
            var specifications = new List<QuerySpecification>();

            if (statement is SelectStatement)
            {
                specifications.Add( (statement as SelectStatement).QueryExpression as QuerySpecification );
            }

            specifications.AddRange(SearchChildren(statement));

            return specifications;
        }

        private static IEnumerable<QuerySpecification> SearchChildren(TSqlFragment fragment)
        {
            if (fragment is InsertStatement)
            {
                return SearchChildren((fragment as InsertStatement).InsertSpecification.InsertSource);
            }

            if (fragment is SelectInsertSource)
            {
                return SearchChildren((fragment as SelectInsertSource).Select);
            }

            var children = new List<QuerySpecification>();
          
            if (fragment is BinaryQueryExpression)
            {
                var expression = fragment as BinaryQueryExpression;
                children.AddRange(SearchChildren(expression.FirstQueryExpression));
                children.AddRange(SearchChildren(expression.SecondQueryExpression));
            }

            if (fragment is QueryParenthesisExpression)
            {
                var expression = fragment as QueryParenthesisExpression;
                children.AddRange(SearchChildren(expression.QueryExpression));
            }

            if (fragment is BooleanBinaryExpression)
            {
                var expression = fragment as BooleanBinaryExpression;
                children.AddRange(SearchChildren(expression.FirstExpression));
                children.AddRange(SearchChildren(expression.SecondExpression));
            }

            if (fragment is BooleanComparisonExpression)
            {
                var expression = fragment as BooleanComparisonExpression;
                children.AddRange(SearchChildren(expression.FirstExpression));
                children.AddRange(SearchChildren(expression.SecondExpression));
            }

            if (fragment is ScalarSubquery)
            {
                var query = fragment as ScalarSubquery;
                children.AddRange(SearchChildren(query.QueryExpression));
            }

            if (fragment is QuerySpecification)
            {
                var spec = fragment as QuerySpecification;
                
                children.Add(spec);
                
                foreach (var select in spec.SelectElements)
                {
                    children.AddRange(SearchChildren(select));
                }

                if (spec.WhereClause != null)
                {
                    children.AddRange(SearchChildren(spec.WhereClause.SearchCondition));
                }

                if (spec.FromClause != null)
                {
                    foreach (var table in spec.FromClause.TableReferences)
                    {
                        children.AddRange(SearchChildren(table));
                    }
                }
                
            }

            return children;
        }
    }
}