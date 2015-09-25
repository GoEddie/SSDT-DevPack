using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnvDTE;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.Enumerators;
using SSDTDevPack.Common.ProjectItems;
using SSDTDevPack.Merge.MergeDescriptor;
using SSDTDevPack.Merge.Parsing;

//  when generatingt merge statements, if unicode column, the N isn't added
//  if no key columns - should put a warning on the datagrid
//  (if i modify the script, make sure it isn't deleted?? - lost change when i manually entered in key columns - didn't push refresh

namespace SSDTDevPack.Merge.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private TreeViewItem _lastNode;

        public MainWindow()
        {
            InitializeComponent();

            if (Assembly.GetCallingAssembly().FullName !=
                "WinFormHost.Merge, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")

                WaitForPopulateTreeView();
        }

        private async void WaitForPopulateTreeView()
        {
            Cursor = Cursors.Wait;
            await Task.Run(() => { PopulateTreeview(); });

            Cursor = Cursors.Arrow;
        }

        private void ClearWindows()
        {
            try
            {
                ProjectItems.Items.Clear();
                if (God.CurrentMergeData != null)
                {
                    God.CurrentMergeData = null;
                    God.Merge = null;

                    if (God.DataTableChanged != null)
                        God.DataTableChanged();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error unable to clear windows: " + e.Message);
            }
        }

        public void PopulateTreeview()
        {
            Dispatcher.Invoke(ClearWindows);

            try
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
            catch (Exception e)
            {
                MessageBox.Show("Error populating tree view: " + e.Message);
            }
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


            node.Tag = new ScriptNodeTag
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

        private void mergeNode_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_lastNode != null && _lastNode.Equals(e.Source as TreeViewItem))
                {
                    return;
                }


                var node = e.Source as TreeViewItem;
                if (node == null)
                {
                    _lastNode = null;
                    ClearTablePage();
                    return;
                }

                var merge = node.Tag as MergeDescriptor.Merge;

                if (merge == null)
                {
                    _lastNode = null;
                    ClearTablePage();
                    return;
                }
                God.Merge = merge;
                God.CurrentMergeData = merge.Data;
                God.DataTableChanged.Invoke();

                _lastNode = node;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting node in tree: " + ex.Message);
            }
        }

        private async void ToolbarRefresh_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            await Task.Run(() => { PopulateTreeview(); });
            Cursor = Cursors.Arrow;
        }

        private void ToolbarSave_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => { Dispatcher.Invoke(SaveCurrent); });
        }

        private void SaveCurrent()
        {
            try
            {
                //save current...
                if (God.Merge.Data.ExtendedProperties.ContainsKey("Changed") &&
                    (bool) God.Merge.Data.ExtendedProperties["Changed"])
                {
                    var writer = new MergeWriter(God.Merge);
                    writer.Write();
                    God.Merge.Data.ExtendedProperties.Remove("Changed");
                }

                foreach (var merge in God.MergesToSave)
                {
                    if (merge.Data.ExtendedProperties.ContainsKey("Changed") &&
                        (bool) merge.Data.ExtendedProperties["Changed"])
                    {
                        var writer = new MergeWriter(merge);
                        writer.Write();
                        merge.Data.ExtendedProperties.Remove("Changed");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message);
            }
        }

        private void MenuItem_AddTableOnClick(object sender, RoutedEventArgs e)
        {
            try
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

                var dialog = new AddTableDialog(tableList);
                dialog.ShowDialog();

                var mergeTable =
                    tables.FirstOrDefault(
                        p => string.Format("{0}.{1}", p.Name.GetSchema(), p.Name.GetName()) == dialog.GetSelectedTable());

                var merge = new MergeStatementFactory().Build(mergeTable, tag.ScriptPath);

                var mergeNode = new TreeViewItem();
                mergeNode.Header = merge.Name.Value;
                mergeNode.Tag = merge;
                mergeNode.ContextMenu = ProjectItems.Resources["TableContext"] as ContextMenu;
                item.Items.Add(mergeNode);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to add new table: " + ex.Message);
            }
        }

        private void TableMenu_Clear(object sender, RoutedEventArgs e)
        {
            try
            {
                if (God.CurrentMergeData == null)
                    return;

                God.CurrentMergeData.Rows.Clear();
                God.DataTableChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to clear table: " + ex.Message);
            }
        }

        private void ImportTable_Click(object sender, RoutedEventArgs e)
        {
            try
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
                merge.Data.ExtendedProperties["Changed"] = true;

                var mergeNode = new TreeViewItem();
                mergeNode.Header = merge.Name.Value;
                mergeNode.Tag = merge;
                mergeNode.ContextMenu = ProjectItems.Resources["TableContext"] as ContextMenu;
                item.Items.Add(mergeNode);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to import table: " + ex.Message);
            }
        }

        private void ImportMultipleTables(object sender, RoutedEventArgs e)
        {
            try
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

                var dialog = new ImportMultipleTablesDialog(tableList);
                dialog.ShowDialog();

                foreach (var import in dialog.GetImportedTables())
                {
                    var mergeTable =
                        tables.FirstOrDefault(
                            p => string.Format("{0}.{1}", p.Name.GetSchema(), p.Name.GetName()) == import.Name);

                    var merge = new MergeStatementFactory().Build(mergeTable, tag.ScriptPath, import.Data);
                    merge.Data.ExtendedProperties["Changed"] = true;
                    var mergeNode = new TreeViewItem();
                    mergeNode.Header = merge.Name.Value;
                    mergeNode.Tag = merge;
                    mergeNode.ContextMenu = ProjectItems.Resources["TableContext"] as ContextMenu;
                    item.Items.Add(mergeNode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to import tables: " + ex.Message);
            }
        }

        private void ImportOverwrite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = ProjectItems.SelectedItem as TreeViewItem;
                if (null == item)
                    return;

                var merge = item.Tag as MergeDescriptor.Merge;
                if (merge == null)
                    return;

                var dialog =
                    new ImportOverwriteTableDialog(string.Format("{0}.{1}", merge.Table.Name.GetSchema(),
                        merge.Table.Name.GetName()));
                dialog.ShowDialog();
                merge.Data.ExtendedProperties["Changed"] = true;
                merge.Data = dialog.ImportedData;
                God.CurrentMergeData = merge.Data;
                God.DataTableChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to import/overwrite table: " + ex.Message);
            }
        }
    }

    internal class ScriptNodeTag
    {
        public string ScriptPath;
        public TableRepository Tables;
    }
}