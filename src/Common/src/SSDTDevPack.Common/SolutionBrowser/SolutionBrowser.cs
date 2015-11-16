using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDTDevPack.Common.SolutionBrowser
{
    public class SolutionBrowser
    {
        private readonly string _projectType;

        public SolutionBrowser(string projectType)
        {
            _projectType = projectType;
        }

        public string ChooseSolutionLocation()
        {
            return "";
        }

    }
}
