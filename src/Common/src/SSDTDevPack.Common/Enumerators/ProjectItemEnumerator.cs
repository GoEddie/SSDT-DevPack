using System.Collections.Generic;
using EnvDTE;

namespace SSDTDevPack.Common.Enumerators
{
    public class ProjectItemEnumerator
    {
        public IList<ProjectItem> Get(Project project)
        {
            var items = new List<ProjectItem>();

            for (var i = 1; i <= project.ProjectItems.Count; i++)
            {
                if (project.ProjectItems.Item(i).ProjectItems != null)
                {
                    items.AddRange(GetChildItems(project.ProjectItems.Item(i)));
                }
            }

            return items;
        }

        private IList<ProjectItem> GetChildItems(ProjectItem item)
        {
            var items = new List<ProjectItem>();

            items.Add(item);

            if (item.ProjectItems == null || item.ProjectItems.Count == 0)
            {
                return items;
            }

            for (var i = 1; i <= item.ProjectItems.Count; i++)
            {
                var childItem = item.ProjectItems.Item(i);
                items.AddRange(GetChildItems(childItem));
            }

            return items;
        }
    }
}