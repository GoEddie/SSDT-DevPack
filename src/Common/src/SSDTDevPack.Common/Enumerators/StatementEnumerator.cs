using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.VSPackage;

namespace SSDTDevPack.Common.Enumerators
{

    public struct CodeStatement<T>
    {
        public string FileName;
        public int StartLocation;
        public int Length;
        public int Line;
        public T Statement;
    }

    public class StatementEnumerator
    {

        public List<CodeStatement<CreateIndexStatement>> GetIndexes(Project project = null)
        {
            if (project != null)
            {
                return GetCodeStatments(project);
            }

            var statements = new List<CodeStatement<CreateIndexStatement>>();
            var enumerator = new ProjectEnumerator();

            foreach (var p in enumerator.Get(ProjectType.SSDT))
            {
                statements.AddRange(GetCodeStatments(p));    
            }

            return statements;
        }

        private List<CodeStatement<CreateIndexStatement>> GetCodeStatments(Project p)
        {
            var enumerator = new ProjectItemEnumerator();
            var items = enumerator.Get(p);

            var statements = new List<CodeStatement<CreateIndexStatement>>();

            foreach (var item in items)
            {
                if (item.Kind.ToUpper() != "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}")
                {
                    continue;
                }

                var filename = item.Properties.Item("FullPath").Value;

                var script = item.Document.GetText();
                if (String.IsNullOrEmpty(script))
                {
                    script = File.ReadAllText(filename);
                }

                if (String.IsNullOrEmpty(script))
                    continue;

                try
                {
                    var indexes = ScriptDom.ScriptDom.GetCreateIndex(script);
                    foreach (var index in indexes)
                    {
                        var codeStatament = new CodeStatement<CreateIndexStatement>();
                        codeStatament.Statement = index;
                        codeStatament.FileName = filename;
                        codeStatament.StartLocation = index.StartOffset;
                        codeStatament.Length = index.FragmentLength;
                        codeStatament.Line = index.StartLine;
                        statements.Add(codeStatament);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log.WriteInfo("Error getting indexes from script in: {0}, error: {1}", item.Name, ex.Message);
                }
            }

            return statements;
        }
    }
}
