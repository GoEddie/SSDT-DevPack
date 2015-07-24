using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using SSDTDevPack.Common.VSPackage;
using SSDTDevPack.Logging;

namespace SSDTDevPack.Common
{
    public class ProjectEnumerator
    {
        public IList<Project> Get(string projectType)
        {
            var dte = VsServiceProvider.Get(typeof (SDTE)) as DTE;

            var projects = new List<Project>();
            for (var i = 1; i <= dte.Solution.Projects.Count; i++)
            {
                var project = dte.Solution.Projects.Item(i);

                Log.WriteInfo("ProjectEnumerator: Have Project: {0}", project.FullName);
                projects.AddRange(GetChildren(projectType, project));
            }
            return projects;
        }

        private IList<Project> GetChildren(string projectType, Project project)
        {
            var projects = new List<Project>();

            for (var i = 1; i <= project.ProjectItems.Count; i++)
            {
                var item = project.ProjectItems.Item(i);
                if (item.SubProject != null)
                {
                    projects.AddRange(GetChildren(projectType, item.SubProject));
                }

                if (new Guid(item.Kind).Equals(new Guid(projectType)))
                {
                    Log.WriteInfo("ProjectEnumerator: Adding Project: {0}", project.FullName);
                    projects.Add(item.SubProject);
                }
                else
                {
                    Log.WriteInfo(
                        "ProjectEnumerator: Not Adding Project: {0} because it is the incorrect kind (kind={1} + wanted={2})",
                        project.FullName, project.Kind, projectType);
                }
            }

            if (new Guid(project.Kind).Equals(new Guid(projectType)))
            {
                Log.WriteInfo("ProjectEnumerator: Adding Project: {0}", project.FullName);
                projects.Add(project);
            }


            return projects;
        }
    }
}