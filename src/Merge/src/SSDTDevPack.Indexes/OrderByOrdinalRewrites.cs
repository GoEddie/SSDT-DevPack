using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;

namespace SSDTDevPack.Rewriter
{
    public class OrderByOrdinalRewrites
    {
        public List<Replacements> GetReplacements(List<QuerySpecification> queries)
        {
            var replacements = new List<Replacements>();

            foreach (var spec in queries)
            {
                if (spec.OrderByClause == null)
                    continue;

                if (spec.OrderByClause.OrderByElements.Any(p => p.Expression is IntegerLiteral))
                {
                    var integerOrderClauses = spec.OrderByClause.OrderByElements.Where(p => p.Expression is IntegerLiteral);

                    var ordinalNames = spec.SelectElements;
                    
                    foreach (var ordinal in integerOrderClauses)
                    {
                        var position = Int32.Parse((ordinal.Expression as IntegerLiteral).Value);
                        for (var i = 0; i < ordinalNames.Count; i++)
                        {
                            if (ordinalNames[i] is SelectStarExpression)
                                return null;  //can't re-write as we don't know what the ordinal relates to :(

                            if (position - 1 == i)
                            {
                                if (!(ordinalNames[i] is SelectScalarExpression))
                                {
                                    return null;    //col ref is something else??
                                }

                                var replacement = new Replacements();
                                replacement.Original = (ordinal.Expression as IntegerLiteral).Value;
                                replacement.OriginalFragment = ordinal;
                                replacement.OriginalLength = ordinal.Expression.FragmentLength;
                                replacement.OriginalOffset = ordinal.Expression.StartOffset;

                                var expression = ordinalNames[i] as SelectScalarExpression;
                                if (expression.ColumnName != null && !String.IsNullOrEmpty(expression.ColumnName.Value))
                                {
                                    replacement.Replacement = expression.ColumnName.Value;
                                }
                                else
                                {
                                    replacement.Replacement =
                                        (expression.Expression as ColumnReferenceExpression).MultiPartIdentifier
                                            .ToNameString();
                                }
                                
                                replacements.Add(replacement);
                                break;
                            }
                        }
                    }

                }
            }
            return replacements;
        } 
    }
}