using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac.Model;

namespace SSDTDevPack.Common.Dac
{
    public static class ObjectIdentifierExtensions
    {
        public static string GetName(this ObjectIdentifier name)
        {
            return name.Parts.LastOrDefault();
        }

        public static string GetSchema(this ObjectIdentifier name)
        {
            if (name.Parts.Count == 3)
            {
                return name.Parts[1];
            }

            if (name.Parts.Count == 2)
            {
                return name.Parts[0];
            }

            return null;
        }

    }
}
