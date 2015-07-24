using NLog;

namespace SSDTDevPack.Logging
{
    public class Log
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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