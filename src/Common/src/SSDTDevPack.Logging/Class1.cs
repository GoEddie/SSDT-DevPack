using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace SSDTDevPack.Logging
{
    public class Log
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void WriteInfo(string message)
        {
                logger.Info(message);
        }

        public static void WriteInfo(string format, params object[] parameters)
        {
            logger.Info(format, parameters);
        }
    }
}
