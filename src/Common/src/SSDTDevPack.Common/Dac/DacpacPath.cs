using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace SSDTDevPack.Common.Dac
{
    public class DacpacPath
    {
        private const string DacpacExtension = ".dacpac";

        public static string Get(Project project)
        {

            var config = project.ConfigurationManager.ActiveConfiguration;
            

            var builtGroup =
                project.ConfigurationManager.ActiveConfiguration.OutputGroups.OfType<OutputGroup>()
                    .First(x => x.CanonicalName == "Built");

            try
            {
                if (builtGroup.FileURLs == null)
                    return null;
            }
            catch (Exception e)
            {
                return null;
            }

            foreach (var strUri in ((object[]) builtGroup.FileURLs).OfType<string>())
            {
                var uri = new Uri(strUri, UriKind.Absolute);
                var filePath = uri.LocalPath;

                if (filePath.EndsWith(DacpacExtension, StringComparison.OrdinalIgnoreCase))
                    return filePath;
            }

            return null;
        }
    }
}