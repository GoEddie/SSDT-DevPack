using System.Windows.Forms;

namespace SSDTDevPack.Merge.UI
{
    public partial class ConnectDialog : Form
    {
        public string ConnectionString;

        public ConnectDialog()
        {
            InitializeComponent();
            
            dialog.SetNotification(ConnectionAvailable);
            
        }

        public void ConnectionAvailable(string connection)
        {
            ConnectionString = connection;
            Close();
        }
    }
}