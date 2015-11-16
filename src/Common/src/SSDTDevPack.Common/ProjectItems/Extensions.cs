using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace SSDTDevPack.Common.ProjectItems
{
    public static class Extensions
    {
        public static bool HasBuildAction(this EnvDTE.ProjectItem item, string buildAction)
        {
            return item.Properties != null && item.Properties.Cast<Property>().Any(property => property.Name == "BuildAction" && property.Value.ToString() == buildAction);
        }

        public static string GetStringProperty(this EnvDTE.ProjectItem item, string propertyName)
        {
            var prop = item.Properties.Cast<Property>().FirstOrDefault(property => property.Name == propertyName);
            if (prop != null)
                return prop.Value;

            return null;
        }
    }
}
