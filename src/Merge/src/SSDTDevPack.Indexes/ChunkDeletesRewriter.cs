using System;
using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.ScriptDom;

namespace SSDTDevPack.Rewriter
{
    public class ChunkDeletesRewriter
    {
        private readonly string _script;

        public ChunkDeletesRewriter(string script)
        {
            _script = script;
        }

        public List<Replacements> GetReplacements(List<DeleteSpecification> statements)
        {
            var replacements = new List<Replacements>();

            foreach (var delete in statements)
            {
                if (delete.TopRowFilter == null)
                {
                    var newStatement = buildChunkedDelete(delete);
                    var replacement = new Replacements();
                    replacement.Original = _script.Substring(delete.StartOffset, delete.FragmentLength);
                    replacement.OriginalFragment = delete;
                    replacement.OriginalLength = delete.FragmentLength;
                    replacement.OriginalOffset = delete.StartOffset;
                    replacement.Replacement = newStatement;
                    replacements.Add(replacement);
                }
            }

            return replacements;
        }

        private string buildChunkedDelete(DeleteSpecification delete)
        {

            var counter = new DeclareVariableStatement();
            var counterVariable = new DeclareVariableElement();
            counterVariable.DataType = new SqlDataTypeReference() {SqlDataTypeOption = SqlDataTypeOption.Int};
            counterVariable.VariableName = new Identifier() {Value = "@rowcount"};
            counterVariable.Value = new IntegerLiteral() {Value = "10000"};
            counter.Declarations.Add(counterVariable);
            
            delete.TopRowFilter = new TopRowFilter();
            delete.TopRowFilter.Expression = new ParenthesisExpression() {Expression = new IntegerLiteral() {Value = "10000"} };

            var setCounter = new SetVariableStatement();
            setCounter.Variable = new VariableReference() {Name = "@rowcount"};
            setCounter.Expression = new GlobalVariableExpression() {Name = "@@rowcount"};
            setCounter.AssignmentKind = AssignmentKind.Equals;

            var deleteStatement = new DeleteStatement();
            deleteStatement.DeleteSpecification = delete;

            var beginEnd = new BeginEndBlockStatement();
            beginEnd.StatementList = new StatementList();
            beginEnd.StatementList.Statements.Add(deleteStatement);
            beginEnd.StatementList.Statements.Add(setCounter);

            var whilePredicate = new BooleanComparisonExpression();
            whilePredicate.ComparisonType = BooleanComparisonType.GreaterThan;
            whilePredicate.FirstExpression = new VariableReference() {Name = "@rowcount"};
            whilePredicate.SecondExpression = new IntegerLiteral() {Value = "0"};

            var whileStatement = new WhileStatement();
            whileStatement.Predicate = whilePredicate;
            whileStatement.Statement = beginEnd;
            
            var text = ScriptDom.GenerateTSql(counter) + "\r\n" + ScriptDom.GenerateTSql(whileStatement);

            return text;
        }
    }
}