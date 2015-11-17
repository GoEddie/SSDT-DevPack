using System;
using System.Windows;
using System.Windows.Controls;
using EnvDTE;
using SSDTDevPack.Common.Enumerators;
using SSDTDevPack.Common.ProjectItems;

namespace SSDTDevPack.Common.SolutionBrowser
{
    /// <summary>
    ///     Interaction logic for wpfSolutionBrowser.xaml
    /// </summary>
    public partial class wpfSolutionBrowser : UserControl
    {
        private ProjectItem _defaultPath;
        private ProjectItem _destinationItem;
        private bool _okClicked;
        private SolutionBrowserForm _parent;

        public wpfSolutionBrowser()
        {
            InitializeComponent();
        }

        public void Fill(SolutionBrowserForm parent, string projectType, string objectName,
            ProjectItem defaultPath = null)
        {
            ObjectName.Text = objectName;
            _parent = parent;
            _defaultPath = defaultPath;

            var enumerator = new ProjectEnumerator();
            foreach (var project in enumerator.Get(projectType))
            {
                var newNode = new TreeViewItem();
                newNode.Header = project.Name;
                newNode.Tag = project;

                for (var i = 1; i <= project.ProjectItems.Count; i++)
                {
                    var expandParent = false;
                    var node = AddChildren(project.ProjectItems.Item(i), out expandParent);

                    if (node != null)
                    {
                        newNode.Items.Add(node);
                        if (expandParent)
                        {
                            newNode.IsExpanded = true;
                            node.IsExpanded = true;
                        }
                    }
                }

                Tree.Items.Add(newNode);
            }
        }

        private TreeViewItem AddChildren(ProjectItem item, out bool expandParent)
        {
            expandParent = false;

            if (item.Name != null && item.Name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                //dirty, should examine the parent...
            {
                return null;
            }

            var node = new TreeViewItem();
            node.Header = item.Name;
            node.Tag = item;

            if (_defaultPath != null && _defaultPath.GetStringProperty("FullPath") == item.GetStringProperty("FullPath"))
            {
                node.IsSelected = true;
                node.IsExpanded = true;
                _destinationItem = item;
                expandParent = true;
            }

            if (item.ProjectItems == null)
                return node;

            var resetExpandParent = false;
            var expandThis = expandParent;

            for (var i = 1; i <= item.ProjectItems.Count; i++)
            {
                var child = item.ProjectItems.Item(i);
                if (!child.Name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                {
                    var newNode = AddChildren(child, out expandParent);
                    node.Items.Add(newNode);
                    if (expandParent)
                    {
                        node.IsExpanded = true;
                        resetExpandParent = true;
                    }
                }
            }

            expandParent = expandThis || resetExpandParent;
            return node;
        }

        public ProjectItem GetDestinationItem()
        {
            if (_okClicked)
                return _destinationItem;

            return null;
        }

        public string GetObjectName()
        {
            return ObjectName.Text;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            _okClicked = true;

            var node = Tree.SelectedValue as TreeViewItem;
            if (node == null)
            {
                _parent.Close();
                return;
            }

            var item = node.Tag as ProjectItem;
            if (item != null)
            {
                _destinationItem = item;
            }

            _parent.Close();
        }
    }
}