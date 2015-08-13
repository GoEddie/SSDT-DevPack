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

namespace SSDTDevPack.Merge.UI
{
    /// <summary>
    /// Interaction logic for TablePage.xaml
    /// </summary>
    public partial class TablePage : UserControl
    {
        public TablePage()
        {
            InitializeComponent();
            
            God.DataTableChanged += () =>
            {
                Grid.ItemsSource = null;

                if (God.CurrentMergeData != null)
                    Grid.ItemsSource = God.CurrentMergeData.DefaultView;
            };
        }
    }
}
