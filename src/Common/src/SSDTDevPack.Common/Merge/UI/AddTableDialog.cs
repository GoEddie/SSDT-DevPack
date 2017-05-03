using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace SSDTDevPack.Merge.UI
{
    public partial class AddTableDialog : Form
    {
        private readonly List<string> _tableList;

        public AddTableDialog(List<string> tableList)
        {
            _tableList = tableList;
            InitializeComponent();

           
        }

        private void AddFileDialog_Load(object sender, EventArgs e)
        {
            foreach (var table in _tableList)
            {
                tableListDropDown.Items.Add(table);
            }
        }

        public string GetSelectedTable()
        {
            if (!_allow)
                return null;

            if (tableListDropDown.SelectedIndex < 0)
                return null;

            return tableListDropDown.Items[tableListDropDown.SelectedIndex] as string;
        }

        private void tableListDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        bool _allow = false;

        private void button1_Click(object sender, EventArgs e)
        {
            _allow = true;
            this.Close();
        }

    }
}
