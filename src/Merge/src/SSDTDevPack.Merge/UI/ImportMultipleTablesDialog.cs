using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using SqlConnectionTest;

namespace SSDTDevPack.Merge.UI
{
    public partial class ImportMultipleTablesDialog : Form
    {
        private readonly List<ImportedTable> _importedTables = new List<ImportedTable>();
        private readonly List<string> _tableList;
        private bool _lastCheckState;

        public ImportMultipleTablesDialog(List<string> tableList)
        {
            InitializeComponent();
            _tableList = tableList;

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
                        buildButton.PerformClick();
                    }
                }
            };
        }

        public List<ImportedTable> GetImportedTables()
        {
            return _importedTables;
        }

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

        private void button1_Click(object sender, EventArgs e)
        {
            var connectionDialog = new ConnectDialog();
            connectionDialog.ShowDialog();

            if (!string.IsNullOrEmpty(connectionDialog.ConnectionString))
            {
                connectionString.Text = connectionDialog.ConnectionString;
            }
        }

        private void import_Click(object sender, EventArgs e)
        {
            foreach (string checkedTable in tableListDropDown.CheckedItems)
                try
                {
                    using (var con = new SqlConnection(connectionString.Text))
                    {
                        con.Open();
                        using (var cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "select * from " + checkedTable;
                            var reader = cmd.ExecuteReader();
                            var dataTable = new DataTable();
                            dataTable.Load(reader);

                            _importedTables.Add(new ImportedTable {Data = dataTable, Name = checkedTable});
                            
                            dataTable.RowChanged += (s, args) =>
                            {
                                dataTable.ExtendedProperties["Changed"] = true;
                            };

                            dataTable.TableNewRow += (s, args) =>
                            {
                                dataTable.ExtendedProperties["Changed"] = true;
                            };

                            dataTable.RowDeleting += (s, args) =>
                            {
                                dataTable.ExtendedProperties["Changed"] = true;
                            };

                            Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error importing tables: " + ex.Message);
                    _importedTables.Clear();
                    return;
                }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _lastCheckState = !_lastCheckState;

            for (var i = 0; i < tableListDropDown.Items.Count; i++)
            {
                tableListDropDown.SetItemChecked(i, _lastCheckState);
            }
        }
    }

    public class ImportedTable
    {
        public DataTable Data;
        public string Name;
    }
}