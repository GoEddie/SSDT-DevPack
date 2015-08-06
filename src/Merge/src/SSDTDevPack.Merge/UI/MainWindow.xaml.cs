using System;
using System.Collections.Generic;
using System.IO;
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
using SSDTDevPack.Common.ProjectItems;

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

        private void ClearWindows()
        {
            ProjectItems.Items.Clear();
        }

        public void PopulateTreeview()
        {
            Dispatcher.Invoke(ClearWindows);

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
            var root = Dispatcher.Invoke(() => new TreeViewItem());
            Dispatcher.Invoke(() => CreateProjectDefaults(root, project));
            


            return root;
        }

        private void CreateProjectDefaults(TreeViewItem root, Project project)
        {
            root.Header = project.Name;

            var preDeploy = GetScripts(project, "PreDeploy");
            var postDeploy = GetScripts(project, "PostDeploy");
            var other = GetScripts(project, "None");

            root.Items.Add(preDeploy);
            root.Items.Add(postDeploy);
            root.Items.Add(other);
            
        }

        private TreeViewItem GetScripts(Project project, string type)
        {
            var node = new TreeViewItem();
            node.Header = type;
            
            foreach (ProjectItem item in new ProjectItemEnumerator().Get(project).Where(p => p.HasBuildAction(type)))
            {
                node.Items.Add(GetScriptNode(item));
            }
            
            return node;

        }

        private TreeViewItem GetScriptNode(ProjectItem item)
        {
            var node = new TreeViewItem();
            node.Header = item.Name;

            //parse the merge statements...

            return node;
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
