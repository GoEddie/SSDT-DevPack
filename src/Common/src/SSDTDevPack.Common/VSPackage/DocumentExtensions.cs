using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace SSDTDevPack.Common.VSPackage
{
    public static class DocumentExtensions
    {
        public static string GetText(this Document source)
        {
            if (source == null)
                return null;    //can this even happen?

            var doc = source.Object("TextDocument") as TextDocument;

            if (null == doc)
            {
                return null;
            }

            var ep = doc.StartPoint.CreateEditPoint();
            ep.EndOfDocument();

            var length = ep.AbsoluteCharOffset;
            ep.StartOfDocument();
            return ep.GetText(length);
        }
        

    }
}
