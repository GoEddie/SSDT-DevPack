using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EnvDTE;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.Enumerators;
using SSDTDevPack.Common.ProjectItems;
using SSDTDevPack.Merge.MergeDescriptor;
using SSDTDevPack.Merge.Parsing;

namespace SSDTDevPack.Merge.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            if (Assembly.GetCallingAssembly().FullName !=
                "WinFormHost.Merge, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")
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
            root.Selected += mergeNode_Selected;

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

            foreach (var item in new ProjectItemEnumerator().Get(project).Where(p => p.HasBuildAction(type)))
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
                var mergeNode = new TreeViewItem();
                mergeNode.Header = merge.Name.Value;
                mergeNode.Tag = merge;

                node.Items.Add(mergeNode);
            }


            return node;
        }

        private void ClearTablePage()
        {
            God.CurrentMergeData = null;
            God.DataTableChanged.Invoke();
        }

        private void mergeNode_Selected(object sender, RoutedEventArgs e)
        {
            var node = e.Source as TreeViewItem;
            if (node == null)
            {
                ClearTablePage();
                return;
            }

            var merge = node.Tag as MergeDescriptor.Merge;

            if (merge == null)
            {
                ClearTablePage();
                return;
            }
            God.Merge = merge;
            God.CurrentMergeData = merge.Data;
            God.DataTableChanged.Invoke();
        }

        private void ToolbarRefresh_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => { PopulateTreeview(); });
        }

        private void ToolbarSave_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Dispatcher.Invoke(SaveCurrent);
            });
        }

        private void SaveCurrent()
        {
            if (God.Merge == null)
                return;

            var writer = new MergeWriter(God.Merge);
            writer.Write();
            
            //If they go mad clicking around in the ui while we are saving this will likely cause issues....
            if(God.CurrentMergeData != null)
                God.DataTableChanged();
        }

    }
}