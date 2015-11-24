using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                if (t.IsKeyword())
                    builder.Append(t.Text.ToUpper());
                else
                {
                    builder.Append(t.Text);
                }
            }

            return builder.ToString();
        }

        public static string KeywordsToLower(string script)
        {
            var fragment = ScriptDom.GetFragment(script);

            var builder = new StringBuilder();

            foreach (var t in fragment.ScriptTokenStream)
            {
                if (t.IsKeyword())
                    builder.Append(t.Text.ToLower());
                else
                {
                    builder.Append(t.Text);
                }
            }

            return builder.ToString();
        }

    }
}
