using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Settings;

namespace SSDTDevPack.tSQLtStubber
{
    public class ScriptBuilder
    {
        protected string GenerateScript(TSqlFragment fragment)
        {
            string script;
            var generator = new Sql120ScriptGenerator(SavedSettings.Get().GeneratorOptions);
            generator.GenerateScript(fragment, out script);
            return script;
        }

        protected IList<TSqlParserToken> GetTokens(TSqlFragment fragment)
        {
            var generator = new Sql120ScriptGenerator(SavedSettings.Get().GeneratorOptions);
            return generator.GenerateTokens(fragment);
        }
    }
}