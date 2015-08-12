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
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.Enumerators;
using SSDTDevPack.Common.ProjectItems;
using SSDTDevPack.Merge.Parsing;

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

            var tables = new TableRepository(DacpacPath.Get(project));

            var preDeploy = GetScripts(project, tables, "PreDeploy");
            var postDeploy = GetScripts(project, tables, "PostDeploy");
            var other = GetScripts(project, tables, "None");

            root.Items.Add(preDeploy);
            root.Items.Add(postDeploy);
            root.Items.Add(other);
            
        }

        private TreeViewItem GetScripts(Project project, TableRepository tables, string type)
        {
            var node = new TreeViewItem();
            node.Header = type;
            
            foreach (ProjectItem item in new ProjectItemEnumerator().Get(project).Where(p => p.HasBuildAction(type)))
            {
                node.Items.Add(GetScriptNode(tables, item));
            }
            
            return node;

        }

        private TreeViewItem GetScriptNode(TableRepository tables, ProjectItem item)
        {
            var node = new TreeViewItem();
            node.Header = item.Name;

            //parse the merge statements...
            var repoitory = new MergeStatementRepository(tables, item.FileNames[0]);
            repoitory.Populate();

            foreach (var merge in repoitory.Get())
            {
                node.Items.Add(merge.Name.Value);
            }


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
