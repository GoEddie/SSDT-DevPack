using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using EnvDTE;
using SSDTDevPack.Common.Enumerators;

namespace SSDTDevPack.Merge.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            if (Assembly.GetCallingAssembly().FullName != "WinFormHost.Merge, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")
                PopulateTreeview();
        }

        public void PopulateTreeview()
        {
            var projects = new ProjectEnumerator().Get(ProjectType.SSDT);

            var root = Dispatcher.Invoke(() => new TreeViewItem());
            
            
            foreach (var project in projects)
            {
                var node = GetProjectNode(project);
                Dispatcher.Invoke(() => root.Items.Add(node));
            }

            Dispatcher.Invoke(() =>
            {
                root.Header = "Solution";
                ProjectItems.Items.Add(root);
            });

        }

        private TreeViewItem GetProjectNode(Project project)
        {
            var root = new TreeViewItem();
            root.Header = project.Name;
            return root;
        }

        private void ToolbarRefresh_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                PopulateTreeview();
            });
        }
    }
}
