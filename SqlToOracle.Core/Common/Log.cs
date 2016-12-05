using log4net;

namespace SqlToOracle.Core.Common
{
    public class Logger
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Error(object message)
        {
            Log.Error(message);
        }

        public static void Info(object message)
        {
            Log.Info(message);
        }
    }
}
