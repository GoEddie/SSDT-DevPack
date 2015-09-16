using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SqlConnectionTest;

namespace SSDTDevPack.Merge.UI
{
    public partial class ImportSingleTableDialog : Form
    {
       private readonly List<string> _tableList;
        
       private void ImportSingleTable_Load(object sender, EventArgs e)
        {
            foreach (var table in _tableList)
            {
                tableListDropDown.Items.Add(table);
            }
        }

        public string GetSelectedTable()
        {
            if (tableListDropDown.SelectedIndex < 0)
                return null;

            return tableListDropDown.Items[tableListDropDown.SelectedIndex] as string;
        }

        public ImportSingleTableDialog(List<string> tableList)
        {
            InitializeComponent();
             _tableList = tableList;

            if (!String.IsNullOrEmpty(UiSettings.ConnectionString))
            {
                this.connectionString.Text = UiSettings.ConnectionString;
            }

            Closing += (sender, args) =>
            {
                if (!String.IsNullOrEmpty(this.connectionString.Text))
                {
                    UiSettings.ConnectionString = this.connectionString.Text;
                }
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var connectionDialog = new ConnectDialog();
            connectionDialog.ShowDialog();

            if (!String.IsNullOrEmpty(connectionDialog.ConnectionString))
            {
                this.connectionString.Text = connectionDialog.ConnectionString;
            }
        }

        public DataTable ImportedData { get; private set; }

        private void import_Click(object sender, EventArgs e)
        {
            try
            {
                using (var con = new SqlConnection(connectionString.Text))
                {
                    con.Open();
                    using (var cmd = con.CreateCommand())
                    {
                        var table = GetSelectedTable();
                        if (string.IsNullOrEmpty(table))
                            return;

                        cmd.CommandText = "select * from " + table;
                        var reader = cmd.ExecuteReader();
                        var dataTable = new DataTable();
                        dataTable.Load(reader);
                        ImportedData = dataTable;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        
    }
}
