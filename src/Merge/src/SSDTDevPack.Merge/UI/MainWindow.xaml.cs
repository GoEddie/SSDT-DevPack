using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EnvDTE;
using Microsoft.SqlServer.TransactSql.ScriptDom;
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
            if (God.CurrentMergeData != null)
            {
                God.CurrentMergeData = null;
                God.Merge = null;
                
                if(God.DataTableChanged != null)
                    God.DataTableChanged();
            }
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
                mergeNode.ContextMenu = ProjectItems.Resources["TableContext"] as ContextMenu;
                node.Items.Add(mergeNode);
            }

            node.ContextMenu = ProjectItems.Resources["FileContext"] as ContextMenu;

            if (item.Properties.Item("FullPath") == null)
            {
                MessageBox.Show("Error unable to get propert FullPath on script file, unable to build tree");
                return null;
            }


            node.Tag = new ScriptNodeTag()
            {
                ScriptPath = item.Properties.Item("FullPath").Value.ToString(),
                Tables = tables
            }; 

            return node;
        }

        private void ClearTablePage()
        {
            God.CurrentMergeData = null;
            God.DataTableChanged.Invoke();
        }

        private TreeViewItem _lastNode;

        private void mergeNode_Selected(object sender, RoutedEventArgs e)
        {
            if (_lastNode != null && _lastNode.Equals(e.Source as TreeViewItem))
            {
                return;
            }

            //Not really happy with this - will leave it for now
            //if (God.CurrentMergeData != null && God.CurrentMergeData.GetChanges() != null)
            //{
            //    var result =
            //        MessageBox.Show(
            //            "You have unsaved changes to the current merge statement, do you want to save them? Press Yes to Save, No to discard or Cancel",
            //            "Save?", MessageBoxButton.YesNoCancel);
            //    switch (result)
            //    {
            //        case MessageBoxResult.Cancel:
            //            return;

            //        case MessageBoxResult.No:
            //            God.CurrentMergeData.RejectChanges();
            //            break;

            //        case MessageBoxResult.Yes:
            //            SaveCurrent();
            //            break;
            //    }
            //}

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

            _lastNode = node;
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

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var item = ProjectItems.SelectedItem as TreeViewItem;
            if (null == item)
                return;

            var tag = item.Tag as ScriptNodeTag;

            if (tag == null)
            {
                return;
            }
            

            var tableRepository = tag.Tables;

            if (tableRepository == null)
            {
                MessageBox.Show("Unable to read table repository - try refreshing");
                return;
            }

            var tables = tableRepository.Get().OrderBy(p=>p.Name.GetSchema() + p.Name.GetName());
            var tableList = new List<string>();

            foreach (var table in tables)
            {
                tableList.Add(string.Format("{0}.{1}", table.Name.GetSchema(), table.Name.GetName()));
            }

            var dialog = new AddTableDialog(tableList);
            dialog.ShowDialog();

            var mergeTable =
                tables.FirstOrDefault(
                    p => string.Format("{0}.{1}", p.Name.GetSchema(), p.Name.GetName()) == dialog.GetSelectedTable());

            var merge = new MergeStatementFactory().Build(mergeTable, tag.ScriptPath);
            God.Merge = merge;
            God.CurrentMergeData = merge.Data;
            God.DataTableChanged();

            var mergeNode = new TreeViewItem();
            mergeNode.Header = merge.Name.Value;
            mergeNode.Tag = merge;
            mergeNode.ContextMenu = ProjectItems.Resources["TableContext"] as ContextMenu;
            item.Items.Add(mergeNode);

        }


        private void TableMenu_Clear(object sender, RoutedEventArgs e)
        {
            if (God.CurrentMergeData == null)
                return;

            God.CurrentMergeData.Rows.Clear();
            God.DataTableChanged();

        }

        private void ImportTable_Click(object sender, RoutedEventArgs e)
        {
            var item = ProjectItems.SelectedItem as TreeViewItem;
            if (null == item)
                return;

            var tag = item.Tag as ScriptNodeTag;

            if (tag == null)
            {
                return;
            }

            var tableRepository = tag.Tables;

            if (tableRepository == null)
            {
                MessageBox.Show("Unable to read table repository - try refreshing");
                return;
            }

            var tables = tableRepository.Get().OrderBy(p => p.Name.GetSchema() + p.Name.GetName());
            var tableList = new List<string>();

            foreach (var table in tables)
            {
                tableList.Add(string.Format("{0}.{1}", table.Name.GetSchema(), table.Name.GetName()));
            }

            var dialog = new ImportSingleTableDialog(tableList);
            dialog.ShowDialog();

            var mergeTable =
                tables.FirstOrDefault(
                    p => string.Format("{0}.{1}", p.Name.GetSchema(), p.Name.GetName()) == dialog.GetSelectedTable());

            if (dialog.ImportedData == null)
            {
                return;
            }

            var merge = new MergeStatementFactory().Build(mergeTable, tag.ScriptPath, dialog.ImportedData);
            God.Merge = merge;
            God.CurrentMergeData = merge.Data;
            God.DataTableChanged();

            var mergeNode = new TreeViewItem();
            mergeNode.Header = merge.Name.Value;
            mergeNode.Tag = merge;
            mergeNode.ContextMenu = ProjectItems.Resources["TableContext"] as ContextMenu;
            item.Items.Add(mergeNode);
        }
    }

    class ScriptNodeTag
    {
        public string ScriptPath;
        public TableRepository Tables;
    }
}