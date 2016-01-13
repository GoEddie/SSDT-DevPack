using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using SSDTDevPack.Common.VSPackage;

namespace SSDTDevPack.Common.Ui
{
    /// <summary>
    ///     Interaction logic for LabelWithProgressIndicator.xaml
    /// </summary>
    public partial class LabelWithProgressIndicator : UserControl
    {
        private readonly string _documentPath;
        private readonly string _label;
        private readonly double _progress;

        public LabelWithProgressIndicator()
        {
            InitializeComponent();
        }

        public LabelWithProgressIndicator(string label, double progress, string documentPath = null)
        {
            InitializeComponent();
            _label = label;
            _progress = progress;
            _documentPath = documentPath;
        }

        public void Configure()
        {
            Label.Text = _label;
            ToolTip = string.Format("{0}%", _progress);

            if (_progress == 0)
            {
                Background.Visibility = Visibility.Hidden;
            }
            else
            {
                Progress.Width = Background.Width*(_progress/100);
            }
        }

        private void Navigate(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(_documentPath))
                return;

            var dte = VsServiceProvider.Get(typeof (SDTE)) as DTE;

            dte?.Documents.Open(_documentPath);
        }

        private void CheckEnterAndNavigate(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Navigate(sender, null);
        }
    }
}