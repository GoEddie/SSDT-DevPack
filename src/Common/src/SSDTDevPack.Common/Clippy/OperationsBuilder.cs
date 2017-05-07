using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.VisualStudio.Text;
using SSDTDevPack.Common.ScriptDom;
using SSDTDevPack.Common.UserMessages;
using SSDTDevPack.Rewriter;

namespace SSDTDevPack.Clippy
{
    public class  GlyphDefinition
    {
        public GlyphDefinition()
        {
            
        }

        public int Line;
        public int LineCount;
        public int StatementOffset;
        public int StatementLength;
        public readonly List<MenuDefinition> Menu = new List<MenuDefinition>();
        public GlyphDefinitonType Type;

        private string _key;
        public ClippyTag Tag;
        

        public void GenerateKey()
        {
            _key = Line.ToString() + Type.ToString() + Menu.Count;
        }

        public string GetKey()
        {
            return _key;
        }
    }

    public enum GlyphDefinitonType
    {
        Normal,
        Error
    }
    

    public class OperationsBuilder
    {
        private readonly ITextSnapshot _snapshot;
        private readonly TagStore _store;

        public OperationsBuilder(ITextSnapshot snapshot, TagStore store)
        {
            _snapshot = snapshot;
            _store = store;
        }


        public List<GlyphDefinition> GetStatementOptions(string script)
        {
            var defintions = new List<GlyphDefinition>();

            if (script.IndexOf("clippy", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                //show all :)   
                return GetClippyMenuItems();
            }
            
            var statements = _store.GetStatements(script);
            foreach (var s in statements)
            {

                foreach (var m in s.Menu)
                {
                    if(m.Operation is ClippyReplacementOperation)
                        (m.Operation as ClippyReplacementOperation).Configure(_snapshot);
                    else if (m.Operation is ClippyReplacementOperations)
                    {
                        (m.Operation as ClippyReplacementOperations).Configure(_snapshot);
                    }

                }

            }

            return statements;

            //foreach (var error in errors)
            //{
            //    var errorDefinition = new GlyphDefinition();
            //    errorDefinition.Type = GlyphDefinitonType.Error;
            //    errorDefinition.Line = error.Line;
            //    errorDefinition.GenerateKey();
            //    defintions.Add(errorDefinition);
            //}


            return defintions;
        }

        private List<GlyphDefinition> GetClippyMenuItems()
        {
            var d = new GlyphDefinition();
            d.Line = 1;
            d.LineCount = 1;
            d.Menu.Add(new MenuDefinition(){Action = () => { MessageBox.Show("Now"); }, Caption = "When is it beer time?", Type = MenuItemType.MenuItem, Glyph =  d});
            d.Menu.Add(new MenuDefinition() { Action = () => { MessageBox.Show("Now"); }, Caption = "When is it lunch time?", Type = MenuItemType.MenuItem, Glyph = d });

            return new List<GlyphDefinition>(){d};
        }

       


        private void GetIsNullReplacements(List<Replacements> replacements)
        {

            

            foreach (var replacement in replacements)
            {
               
            }
        }
    }
}
