using log4net;

namespace ThresholdAnalysis.Utils
{
    public class Logger
    {
        public static readonly ILog LogExceptions = LogManager.GetLogger("ExceptionAppender");
        public static readonly ILog LogEngineStats = LogManager.GetLogger("EngineStatsAppender");

        public static void Init()
        {
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}
