using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SSDTDevPack.Common.ConnectionDialog
{
    /// <summary>
    /// Interaction logic for SqlConnectionDialog.xaml
    /// </summary>
    public partial class SqlConnectionDialog : UserControl
    {
        private readonly Action<string> _completeNotification;

        public SqlConnectionDialog(Action<string> completeNotification)
        {
            _completeNotification = completeNotification;
            InitializeComponent();
            Database.Focus();
            
        }

        private string _connectionString;

        public string GetConnectionString()
        {
            return _connectionString;
        }

        private void Connect_OnClick(object sender, RoutedEventArgs e)
        {
            _connectionString = string.Format("SERVER={0};{1};", SearchTextBox.Text,
                ((String.IsNullOrEmpty(TextUser.Text)
                    ? "Integrated Security=SSPI"
                    : string.Format("UID={0};PWD={1};", TextUser.Text, TextPass.Text))));

            Database.Items.Clear();

            Cursor = Cursors.Wait;
            TestConnection.IsEnabled = false;

            Task.Run(() =>
            {
                try
                {
                    using (var con = new SqlConnection(_connectionString))
                    {

                        con.Open();
                        using (var cmd = con.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = "select name from sys.databases order by name";
                            using (var sdr = cmd.ExecuteReader())
                            {
                                while (sdr.Read())
                                {
                                    var name = sdr[0].ToString();
                                    Dispatcher.Invoke(() =>  Database.Items.Add(name));
                                }
                            }
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        Database.Focus();
                        Database.IsDropDownOpen = true;
                        TestConnection.IsEnabled = true;
                    });


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error connecting: " + ex.Message);
                }

                Dispatcher.Invoke(() => Cursor = Cursors.Arrow);
            });

        }

        private void SearchTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.SystemKey == Key.Enter || e.Key == Key.Enter)
            {
                Connect_OnClick(sender, new RoutedEventArgs());
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            _connectionString = string.Format("SERVER={0};{1};Initial Catalog={2};", SearchTextBox.Text,
               ((String.IsNullOrEmpty(TextUser.Text)
                   ? "IntegratedSecurity=SSPI"
                   : string.Format("UID={0};PWD={1};", TextUser.Text, TextPass.Text)
            )), Database.Text);


            _completeNotification(_connectionString);

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _completeNotification(null);
        }
    }
}
