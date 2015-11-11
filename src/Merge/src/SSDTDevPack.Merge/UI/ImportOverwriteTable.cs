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

namespace SSDTDevPack.Merge.UI
{
    public partial class ImportOverwriteTableDialog : Form
    {
        private readonly string _table;
        private readonly List<string> _tableList;
        
       private void ImportSingleTable_Load(object sender, EventArgs e)
        {
           
        }


       public ImportOverwriteTableDialog(string table)
        {
           _table = table;
           InitializeComponent();


           if (!string.IsNullOrEmpty(UiSettings.ConnectionString))
           {
               connectionString.Text = UiSettings.ConnectionString;
           }

           Closing += (sender, args) =>
           {
               if (!string.IsNullOrEmpty(connectionString.Text))
               {
                   UiSettings.ConnectionString = connectionString.Text;
               }
           };

           connectionString.KeyDown += (sender, args) =>
           {
               if (args.KeyCode == Keys.Enter)
               {
                   if (string.IsNullOrEmpty(connectionString.Text))
                   {
                       button1.PerformClick();
                   }
               }
           };
        }

        private bool _allow = false;

        private void button1_Click(object sender, EventArgs e)
        {

            _allow = true;

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
                        var table = _table;
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
                MessageBox.Show("Error overwriting existing table: " + ex.Message);
            }
        }

        
    }
}
