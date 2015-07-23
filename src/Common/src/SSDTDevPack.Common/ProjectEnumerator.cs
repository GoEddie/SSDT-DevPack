using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using SSDTDevPack.Common.VSPackage;

namespace SSDTDevPack.Common
{
    public class ProjectEnumerator
    {
        public IList<Project> Get(string type)
        {
            var dte = VsServiceProvider.Get(typeof(SDTE)) as DTE;

            var projects = new List<Project>();
            for (int i = 0; i < dte.Solution.Projects.Count; i++)
            {
                projects.Add(dte.Solution.Projects.Item(i));
            }
            return projects;
        } 

    }
}
