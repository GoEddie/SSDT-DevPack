using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.ScriptDom;

namespace SSDTDevPack.Rewriter
{
    public class NonSargableRewrites
    {
        private readonly string _script;
        private TSqlFragment _currentFragment;
        private List<Replacements> _replacementsToMake = new List<Replacements>();

        public NonSargableRewrites(string script)
        {
            _script = script;
        }

        public List<Replacements> GetReplacements(List<QuerySpecification> queries)
        {
            foreach (var select in queries)
            {
                _currentFragment = select;

                if (select.WhereClause != null)
                {
                    Search(select.WhereClause.SearchCondition);
                }

                if (select.FromClause != null)
                {
                    foreach (var reference in select.FromClause.TableReferences)
                    {
                        if (reference is QualifiedJoin)
                        {
                            var join = reference as QualifiedJoin;
                            Search(join.SearchCondition);
                        }
                    }
                }

            }

            var distinctor = new Dictionary<string, Replacements>();

            foreach (var p in _replacementsToMake)
            {
                distinctor[p.Original + ":" + p.OriginalLength + ":" + p.OriginalOffset] = p;
            }
            //remove duplicates
            _replacementsToMake = distinctor.Values.ToList();

            for (var i = 0; i < _replacementsToMake.Count; i++)
            {
                var rep = _replacementsToMake[i];

                if (!rep.Original.Contains("\r\n") && rep.Replacement.Contains("\r\n"))
                {
                    rep.Replacement = rep.Replacement.Replace("\r\n", "");
                    _replacementsToMake[i] = rep;
                }
            }

            return _replacementsToMake;
        }

        private void Search(BooleanExpression search)
        {
            if (search is BooleanBinaryExpression)
            {
                var bbe = search as BooleanBinaryExpression;
                Search(bbe.FirstExpression);
                Search(bbe.SecondExpression);
            }

            if (search is BooleanParenthesisExpression)
            {
                var bpe = search as BooleanParenthesisExpression;
                Search(bpe.Expression);
            }

            if (search is BooleanComparisonExpression)
            {
                CheckRewriteable(search);
            }
        }

        private void CheckRewriteable(BooleanExpression search)
        {
            var bce = search as BooleanComparisonExpression;
            var haveIsNull = false;
            var haveLiteral = false;

            Literal literal = new BinaryLiteral();
            var isNull = new FunctionCall();

            if (bce.FirstExpression is FunctionCall)
            {
                var func = bce.FirstExpression as FunctionCall;
                if (func.FunctionName.Value.ToLower() == "isnull")
                {
                    haveIsNull = true;
                    isNull = func;
                }
            }

            if (bce.SecondExpression is FunctionCall)
            {
                var func = bce.SecondExpression as FunctionCall;
                if (func.FunctionName.Value.ToLower() == "isnull")
                {
                    haveIsNull = true;
                    isNull = func;
                }
            }

            if (bce.FirstExpression is Literal)
            {
                haveLiteral = true;
                literal = bce.FirstExpression as Literal;
            }

            if (bce.SecondExpression is Literal)
            {
                haveLiteral = true;
                literal = bce.SecondExpression as Literal;
            }

            if (haveLiteral && haveIsNull)
            {
                var firstParam = isNull.Parameters.FirstOrDefault();
                if (!(firstParam is ColumnReferenceExpression))
                {
                    return;
                }

                var secondParam = isNull.Parameters.LastOrDefault();
                if (secondParam is Literal)
                {
                    if (secondParam.GetType() != literal.GetType())
                    {
                        return;
                    }

                    var isSameLiteral = (secondParam as Literal).Value == literal.Value;

                    var isEquals = bce.ComparisonType == BooleanComparisonType.Equals;

                    if (isEquals && isSameLiteral)
                    {
                        BuildEqualsSameLiteral(search, firstParam, literal);

                        return;
                    }

                    if (!isEquals && !isSameLiteral)
                    {
                        BuildNotEqualsNotSameLiteral(search, firstParam, literal);

                        return;
                    }


                    if (!isEquals && isSameLiteral)
                    {
                        BuildNotEqualsIsSameLiteral(search, firstParam, literal);

                        return;
                    }

                    if (isEquals && !isSameLiteral)
                    {
                        BuildIsEqualsIsNotSameLiteral(search, firstParam, literal);
                    }
                }
            }
        }

        private void BuildIsEqualsIsNotSameLiteral(BooleanExpression search, ScalarExpression firstParam,
            Literal literal)
        {
            var newExpression = new BooleanParenthesisExpression();

            var second = new BooleanComparisonExpression();
            second.FirstExpression = firstParam;
            second.SecondExpression = literal;


            newExpression.Expression = second;

            var sql = ScriptDom.GenerateTSql(newExpression);

            _replacementsToMake.Add(new Replacements
            {
                Original = _script.Substring(search.StartOffset, search.FragmentLength),
                OriginalLength = search.FragmentLength,
                OriginalOffset = search.StartOffset,
                Replacement = sql,
                OriginalFragment = _currentFragment
            });
        }

        private void BuildNotEqualsIsSameLiteral(BooleanExpression search, ScalarExpression firstParam,
            Literal literal)
        {
            var newExpression = new BooleanParenthesisExpression();
            var expression = new BooleanBinaryExpression();
            newExpression.Expression = expression;

            expression.BinaryExpressionType = BooleanBinaryExpressionType.And;

            var isnull = new BooleanIsNullExpression();
            isnull.IsNot = true;

            isnull.Expression = firstParam;
            expression.FirstExpression = isnull;

            var second = new BooleanComparisonExpression();
            second.FirstExpression = firstParam;
            second.SecondExpression = literal;
            second.ComparisonType = BooleanComparisonType.NotEqualToBrackets;
            expression.SecondExpression = second;

            var sql = ScriptDom.GenerateTSql(newExpression);

            _replacementsToMake.Add(new Replacements
            {
                Original = _script.Substring(search.StartOffset, search.FragmentLength),
                OriginalLength = search.FragmentLength,
                OriginalOffset = search.StartOffset,
                Replacement = sql,
                OriginalFragment = _currentFragment
            });
        }

        private void BuildNotEqualsNotSameLiteral(BooleanExpression search, ScalarExpression firstParam,
            Literal literal)
        {
            var newExpression = new BooleanParenthesisExpression();
            var expression = new BooleanBinaryExpression();
            newExpression.Expression = expression;

            expression.BinaryExpressionType = BooleanBinaryExpressionType.Or;
            var isnull = new BooleanIsNullExpression();
            isnull.Expression = firstParam;
            expression.FirstExpression = isnull;

            var second = new BooleanComparisonExpression();
            second.FirstExpression = firstParam;
            second.SecondExpression = literal;
            second.ComparisonType = BooleanComparisonType.NotEqualToBrackets;
            expression.SecondExpression = second;

            var sql = ScriptDom.GenerateTSql(newExpression);

            _replacementsToMake.Add(new Replacements
            {
                Original = _script.Substring(search.StartOffset, search.FragmentLength),
                OriginalLength = search.FragmentLength,
                OriginalOffset = search.StartOffset,
                Replacement = sql,
                OriginalFragment = _currentFragment
            });
        }

        private void BuildEqualsSameLiteral(BooleanExpression search, ScalarExpression firstParam, Literal literal)
        {
            var newExpression = new BooleanParenthesisExpression();
            var expression = new BooleanBinaryExpression();
            newExpression.Expression = expression;

            expression.BinaryExpressionType = BooleanBinaryExpressionType.Or;
            var isnull = new BooleanIsNullExpression();
            isnull.Expression = firstParam;
            expression.FirstExpression = isnull;

            var second = new BooleanComparisonExpression();
            second.FirstExpression = firstParam;
            second.SecondExpression = literal;
            expression.SecondExpression = second;

            var sql = ScriptDom.GenerateTSql(newExpression);

            _replacementsToMake.Add(new Replacements
            {
                Original = _script.Substring(search.StartOffset, search.FragmentLength),
                OriginalLength = search.FragmentLength,
                OriginalOffset = search.StartOffset,
                Replacement = sql,
                OriginalFragment = _currentFragment
            });
        }
    }
}