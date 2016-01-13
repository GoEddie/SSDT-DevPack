using System;   
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.ConnectionDialog;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.Enumerators;
using SSDTDevPack.Common.ScriptDom;
using SSDTDevPack.Common.Ui;
using SSDTDevPack.Merge.UI;

namespace SSDTDevPacl.CodeCoverage.Lib.Ui
{
    /// <summary>
    ///     Interaction logic for CodeCoverageWindow.xaml
    /// </summary>
    public partial class CodeCoverageWindow : UserControl
    {
        private string _connectionString;
        private ExtendedEventDataDataReader _reader;

        public CodeCoverageWindow()
        {
            InitializeComponent();
            

        }

        private void Start(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(_connectionString))
            {
                var dialog = new ConnectDialog();
                dialog.ShowDialog();
                _connectionString = dialog.ConnectionString;

                if (String.IsNullOrEmpty(_connectionString))
                    return;
            }

            Task.Run(() =>
            {
                _reader = new ExtendedEventDataDataReader(_connectionString);
                _reader.Start();
            });


            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;

        }

        private void ShowCodeMap(object sender, RoutedEventArgs e)
        {
            CodeMap.Items.Clear();

            var store = CodeCoverageStore.Get;

            var projects = new ProjectEnumerator().Get(ProjectType.SSDT);
            foreach (var p in projects)
            {
                var newItem = new TreeViewItem();
                //newItem.Header = p.Name;

                var statements = new StatementEnumerator().GetStatements(p);
                var fileMap = new Dictionary<string, List<CodeStatement<TSqlStatement>>>();


                foreach (var statement in statements)
                {
                    if (!fileMap.ContainsKey(statement.FileName))
                    {
                        fileMap[statement.FileName] = new List<CodeStatement<TSqlStatement>>();
                    }

                    fileMap[statement.FileName].Add(statement);
                }

                double parentStatements = 0;
                double parentCoveredStatements = 0;

                foreach (var file in fileMap.Keys.OrderBy(pp => pp))
                {
                    var map = fileMap[file];                   

                    var child = new TreeViewItem();
                    double childStatements = 0;
                    double childCoveredStatements = 0;

                   

                    foreach (var sqlModule in map.Where(o => o.Statement.GetType() == typeof (CreateProcedureStatement)))
                    {
                        var name = (sqlModule.Statement as CreateProcedureStatement)?.ProcedureReference.Name.ToNameString();
                        parentStatements = AddChildItems(name, sqlModule, store, parentStatements, file, child, ref parentCoveredStatements, ref childStatements, ref childCoveredStatements);
                        store.AddStatementFileMap(name, sqlModule.FileName);
                    }


                    foreach (var sqlModule in map.Where(o => o.Statement.GetType() == typeof(CreateFunctionStatement)))
                    {
                        var name = (sqlModule.Statement as CreateFunctionStatement)?.Name.ToNameString();
                        parentStatements = AddChildItems(name, sqlModule, store, parentStatements, file, child, ref parentCoveredStatements, ref childStatements, ref childCoveredStatements);
                        store.AddStatementFileMap(name, sqlModule.FileName);
                    }
                    

                    var childCoveragePercent = ((double)childCoveredStatements / (double)childStatements) * 100;
                    var childLabel = new LabelWithProgressIndicator(string.Format("{0} - {1}% ({2} / {3})", new FileInfo(file).Name, childCoveragePercent, childCoveredStatements, childStatements), childCoveragePercent, file);
                    childLabel.Configure();
                    child.Header = childLabel;
                   

                    if (child.Items.Count > 0)
                    {
                        newItem.Items.Add(child);
                    }
                    
                }

                var parentLabel = new LabelWithProgressIndicator(string.Format("{0} - ({1} / {2})", p.Name, parentCoveredStatements, parentStatements), (parentCoveredStatements / parentStatements) * 100.0);
                parentLabel.Configure();
                newItem.Header = parentLabel;

                CodeMap.Items.Add(newItem);
            }
        }

        private static double AddChildItems(string name, CodeStatement<TSqlStatement> sqlModule, CodeCoverageStore store, double parentStatements, string file, TreeViewItem child, ref double parentCoveredStatements, ref double childStatements, ref double childCoveredStatements)
        {
            if (string.IsNullOrEmpty(name))
                return parentStatements;

            //need to enumerate the statement tree to find statements (flatten tree - have probably already done it?? maybe can just use a visitor - see count lines of code surely?)
            var script = File.ReadAllText(sqlModule.FileName);
            if (script.Length < sqlModule.Length)
            {
                //bad tings....
                return parentStatements;
            }
            
            IList<ParseError> errors;
            var statementNodes = ScriptDom.GetStatements(script.Substring(sqlModule.StartLocation, sqlModule.Length), out errors);

            if (errors != null && errors.Count > 1)
            {
                //more bad tings
                return parentStatements;
            }
            
            var coveredStatements = store.GetCoveredStatements(name, sqlModule.FileName);
            
            var statementCount = statementNodes.Count - 1;

            parentStatements += statementCount;
            parentCoveredStatements += coveredStatements?.Count ?? 0;

            childStatements += statementCount;
            childCoveredStatements += coveredStatements?.Count ?? 0;
                                                                                //if the file has changed we can't get anything useful from it...
            if (coveredStatements != null && coveredStatements.Count > 0 && !coveredStatements.Any(p => p.TimeStamp < File.GetLastWriteTimeUtc(file)))
            {
                var coveragePercent = ((double) coveredStatements.Count/(double) statementCount)*100;

                var label = new LabelWithProgressIndicator(string.Format("{0} - {1}% ({2} / {3})", name, coveragePercent, coveredStatements.Count, statementCount), coveragePercent, file);
                label.Configure();

                child.Items.Add(label);
            }
            else
            {
                var label = new LabelWithProgressIndicator(name + " - 0 %", 0, file);
                label.Configure();
                child.Items.Add(label);
            }
            return parentStatements;
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            _reader.Stop();
            var count = _reader.CoveredStatements.Count;

            CodeCoverageStore.Get.AddStatements(_reader.CoveredStatements, _reader.ObjectNameCache);

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            Status.Text = "Added " + count + " statements to code coverage";

            ShowCodeMap(sender, e);
        }

        private void DiscardResults(object sender, RoutedEventArgs e)
        {
            var store = CodeCoverageStore.Get;
            store.ClearStatements();
            ShowCodeMap(sender, e);
        }

        private void ClearConnection(object sender, RoutedEventArgs e)
        {
            _connectionString = String.Empty;
        }
    }
}