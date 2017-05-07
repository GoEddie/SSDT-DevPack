using System;
using System.Collections.Generic;
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

namespace SSDTDevPack.Clippy
{
    /// <summary>
    /// Interaction logic for MenuItem.xaml
    /// </summary>
    public partial class MenuItem : UserControl
    {
        private readonly Action _action;

        public MenuItem(string content, Action action, MainWindow mainWindow)
        {
            _action = action;
            InitializeComponent();

            Title.Text = content;
            
            MouseLeftButtonUp += (sender, args) =>
            {
                if (_action != null)
                {
                    mainWindow.Close();
                    _action();
                }
            };

        }
    }
}
