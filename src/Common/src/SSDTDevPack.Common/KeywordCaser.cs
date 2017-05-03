using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.ScriptDom;

namespace SSDTDevPack.Formatting
{
    public class KeywordCaser
    {


        public static string KeywordsToUpper(string script)
        {
            var fragment = ScriptDom.GetFragment(script);

            var builder = new StringBuilder();

            foreach (var t in fragment.ScriptTokenStream)
            {

                if (string.IsNullOrEmpty(t.Text))
                    continue;

                if (IsKeyword(t))
                    builder.Append(t.Text.ToUpper());
                else
                    builder.Append(t.Text);
                
            }

            return builder.ToString();
        }

        public static string KeywordsToLower(string script)
        {
            var fragment = ScriptDom.GetFragment(script);

            var builder = new StringBuilder();

            foreach (var t in fragment.ScriptTokenStream)
            {
                if (string.IsNullOrEmpty(t.Text))
                    continue;

                if (IsKeyword(t))
                    builder.Append(t.Text.ToLower());
                else
                    builder.Append(t.Text);
                
            }

            return builder.ToString();
        }

        private static readonly List<string> _additionalKeywords = new List<string>(){"RETURNS"}; 

        private static bool IsKeyword(TSqlParserToken sqlParserToken)
        {
            return sqlParserToken.IsKeyword() || _additionalKeywords.Any(p => p == sqlParserToken.Text.ToUpper());
        }
    }
}
