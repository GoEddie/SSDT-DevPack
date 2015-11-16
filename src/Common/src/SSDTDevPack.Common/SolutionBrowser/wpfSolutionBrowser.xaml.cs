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
using EnvDTE;
using SSDTDevPack.Common.Enumerators;

namespace SSDTDevPack.Common.SolutionBrowser
{
    /// <summary>
    /// Interaction logic for wpfSolutionBrowser.xaml
    /// </summary>
    public partial class wpfSolutionBrowser : UserControl
    {
        public wpfSolutionBrowser()
        {
            InitializeComponent();
        }

        private SolutionBrowserForm _parent;

        public void Fill(SolutionBrowserForm parent, string projectType, string objectName)
        {
            ObjectName.Text = objectName;
            _parent = parent;

            var enumerator = new ProjectEnumerator();
            foreach (var project in enumerator.Get(projectType))
            {
                var newNode = new TreeViewItem();
                newNode.Header = project.Name;
                newNode.Tag = project;

                for (var i = 1; i <= project.ProjectItems.Count; i++)
                {
                    var node = AddChildren(project.ProjectItems.Item(i));

                    if(node != null)
                        newNode.Items.Add(node);
                }

                Tree.Items.Add(newNode);
            }
        }

        private TreeViewItem AddChildren(ProjectItem item)
        {
            if (item.Name != null && item.Name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))  //dirty, should examine the parent...
            {
                return null;
            }

            var node = new TreeViewItem();
            node.Header = item.Name;
            node.Tag = item;

            if (item.ProjectItems == null)
                return node;

            for (var i = 1; i <= item.ProjectItems.Count; i++)
            {
                var child = item.ProjectItems.Item(i);
                if (!child.Name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                {
                    var newNode = AddChildren(child);
                    node.Items.Add(newNode);
                }
            }


            return node;
        }

        public ProjectItem DestinationItem { get; private set; }

        public string GetObjectName() {  return ObjectName.Text; } 

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var node = Tree.SelectedValue as TreeViewItem;
            if (node == null)
            {
                _parent.Close();
                return;
            }

            var item = node.Tag as ProjectItem;
            if (item != null)
            {
                DestinationItem = item;
            }

            _parent.Close();
        }
    }
}
