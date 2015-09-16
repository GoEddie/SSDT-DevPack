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

namespace SSDTDevPack.Common.ConnectionDialog
{
    /// <summary>
    /// Interaction logic for LabelledTextbox.xaml
    /// </summary>
    public partial class LabelledTextbox : UserControl
    {
        public LabelledTextbox(string label, string text)
        {
            InitializeComponent();

            Label.Content = label;
            TextBlock.Text = text;
        }

        public string Text()
        {
            return TextBlock.Text;
        }

    }
}
