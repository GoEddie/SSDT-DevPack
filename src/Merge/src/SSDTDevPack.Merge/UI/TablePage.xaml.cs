using System;
using System.Collections.Generic;
using System.Data;
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
        private bool _inUpdate;

        public TablePage()
        {
            InitializeComponent();
            
            God.DataTableChanged += () =>
            {
                Grid.ItemsSource = null;

                DoUpdate.IsChecked = false;
                DoDelete.IsChecked = false;
                DoInsert.IsChecked = false;
                NoKeysWarning.Visibility = Visibility.Hidden;

                if (God.CurrentMergeData != null)
                {
                    if (God.CurrentMergeData.ExtendedProperties.ContainsKey("Changed"))
                    {
                        God.MergesToSave.Add(God.Merge);
                    }
                    
                    Grid.ItemsSource = God.CurrentMergeData.DefaultView;
                    
                    _inUpdate = true;
                    DoUpdate.IsChecked = God.Merge.Option.HasUpdate;
                    DoDelete.IsChecked = God.Merge.Option.HasDelete;
                    DoInsert.IsChecked = God.Merge.Option.HasInsert;

                    if (!God.Merge.Option.HasSearchKeys)
                        NoKeysWarning.Visibility = Visibility.Visible;

                    _inUpdate = false;
                }
            };  

            
        }

        private void DoUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            if (_inUpdate)
                return;

            if (God.Merge != null)
            {
                God.Merge.Option.HasUpdate = DoUpdate.IsChecked.Value;
                SetTableChanged(God.CurrentMergeData);
            }
        }

        private void SetTableChanged(DataTable currentMergeData)
        {
            currentMergeData.ExtendedProperties["Changed"] = true;
        }

        private void DoDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (_inUpdate)
                return;

            if (God.Merge != null)
            {
                God.Merge.Option.HasDelete = DoDelete.IsChecked.Value;
                SetTableChanged(God.CurrentMergeData);
            }
        }
        private void DoInsert_OnClick(object sender, RoutedEventArgs e)
        {
            if (_inUpdate)
                return;

            if (God.Merge != null)
            {
                God.Merge.Option.HasInsert = DoInsert.IsChecked.Value;
                SetTableChanged(God.CurrentMergeData);
            }
        }
    }
}
