using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Logging;

namespace SSDTDevPack.Common.ScriptDom
{
    public class ScriptParser
    {
        private readonly string _path;
        private readonly TSqlFragmentVisitor _visitor;

        public ScriptParser(TSqlFragmentVisitor visitor, string path)
        {
            _visitor = visitor;
            _path = path;
        }

        public void Parse()
        {
            using (var reader = GetScriptReader())
            {
                var parser = new TSql120Parser(true);

                IList<ParseError> errors;
                var sqlFragment = parser.Parse(reader, out errors);

                if (errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        Log.WriteInfo(_path, error.Line, "Script Parser: Error in {0} error: {1}", _path, error.Message);
                    }
                }

                sqlFragment.Accept(_visitor);
            }
        }

        private TextReader GetScriptReader()
        {
            var scriptBuffer = new StringBuilder();

            using (var reader = new StreamReader(_path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line.StartsWith(":"))
                    {
                        scriptBuffer.Append("--");
                    }

                    scriptBuffer.AppendLine(line);
                }
            }

            return new StringReader(scriptBuffer.ToString());
        }
    }
}