using System.Collections.Generic;
using EnvDTE;

namespace SSDTDevPack.QueryCosts
{
    /*
     There are two users of this, firstly when the "Toggle Query Costs" command is run in visual studio, it is within the vspackage so has full access to the DTE
     * so it passes it here so that we can always work with the active document - the second client is the actual highlighter and that has no dte so it uses whatever one is here
     */
    public class DocumentScriptCosters
    {
        private readonly Dictionary<string, ScriptCoster> _costers = new Dictionary<string, ScriptCoster>();

        static readonly DocumentScriptCosters Instance = new DocumentScriptCosters();

        public static DocumentScriptCosters GetInstance()
        {
            return Instance;
        }

        private static DTE _dte;

        public static void SetDte(DTE dte)
        {
            _dte = dte;
        }

        public void ClearCache()
        {

            lock (_costers)
            {
                foreach (var coster in _costers.Values)
                {
                    coster.ShowCosts = false;
                }
                _costers.Clear();
            }
        }

        public ScriptCoster GetCoster()
        {
            lock (_costers){
                if (_dte == null || _dte.ActiveDocument == null || _dte.ActiveDocument.FullName == null)
                {
                    return null;
                }

                if (_costers.ContainsKey(_dte.ActiveDocument.FullName))
                    return _costers[_dte.ActiveDocument.FullName];

                var coster = new ScriptCoster(_dte);
                _costers[_dte.ActiveDocument.FullName] = coster;
                return coster;
            }
        }
    }
}