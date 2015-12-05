using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using SSDTDevPack.Merge.UI;

namespace SSDTDevPack.QueryCosts
{
    public class ScriptCoster
    {
        private static string ConnectionString;
        private QueryCostStore Store;
        private DTE Dte;

        
        public ScriptCoster(DTE dte)
        {
            
            Dte = dte;
        }

        void BuildStore()
        {
            if (String.IsNullOrEmpty(ConnectionString))
            {
                var dialog = new ConnectDialog();
                dialog.ShowDialog();
                ConnectionString = dialog.ConnectionString;

                if (String.IsNullOrEmpty(ConnectionString))
                    return;
            }

            Store = new QueryCostStore(new PlanParser(new QueryCostDataGateway(ConnectionString)));
            ShowCosts = false; //caller flips it first time used
        }

        public List<Statement> GetCosts()
        {
            if (!ShowCosts || Store == null)
                return null;

            if (Dte == null || Dte.ActiveDocument == null || Dte.ActiveDocument.FullName == null)
            {
                return null;
            }

            var activeDocPath = Dte.ActiveDocument.FullName;
            var statements = Store.GetStatements(activeDocPath);
            return statements;
        }

        public void AddCosts(string script, Document doc)
        {
            if (!ShowCosts)
                return;

            if(Store == null)
                BuildStore();

            if (Store == null)
                return;

            Store.AddStatements(script, doc.FullName);
        }

        public bool ShowCosts { get; set; }


    }
}
