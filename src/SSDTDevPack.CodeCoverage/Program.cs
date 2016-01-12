using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSDTDevPacl.CodeCoverage.Lib;

namespace SSDTDevPack.CodeCoverage
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new ExtendedEventDataDataReader("server=(localdb)\\Projectsv12;integrated security=sspi");

            Task.Run(() => c.Start());
            int count = 0;
            while (true)
            {
                while (c.CoveredStatements.IsEmpty)
                {
                    System.Threading.Thread.Sleep(1000);
                }

                CoveredStatement cs;
                if (c.CoveredStatements.TryDequeue(out cs))
                {

                    if (c.ObjectNameCache.ContainsKey(cs.ObjectId))
                    {
                        Console.WriteLine("{0} : {1} : {2} : {3}", c.ObjectNameCache[cs.ObjectId], cs.Offset, cs.Length, cs.ObjectType);
                    }
                    
               
                  //  if (count++ > 10)
                   // {
                     //   c.Stop();
                   // }
                }
            }

        }
    }
}
