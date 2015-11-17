using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using SSDTDevPack.Common.Enumerators;

namespace SSDTDevPack.Common.SolutionBrowser
{
    public partial class SolutionBrowserForm : Form
    {
        private static ProjectItem LastSelectedItem;

        public SolutionBrowserForm(string objectName, ProjectItem defaultSelectedItem = null)
        {
            InitializeComponent();
            wpfSolutionBrowser1.Fill(this, ProjectType.SSDT, objectName, defaultSelectedItem ?? LastSelectedItem);
        }

        public string GetObjectName()
        {
            return wpfSolutionBrowser1.GetObjectName();
        }

        public ProjectItem DestinationItem
        {
            get
            {
                LastSelectedItem = wpfSolutionBrowser1.GetDestinationItem();
                
                return LastSelectedItem;
            }
        }

    }
}
