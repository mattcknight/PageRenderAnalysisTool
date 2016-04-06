using System;
using System.Configuration;
using System.Globalization;
using ThresholdAnalysis.Timeout;
using ThresholdAnalysis.EDHID;
using ThresholdAnalysis.Utils;

namespace ThresholdAnalysis
{
    class Program 
    {
        public static void Process()
        {
            Logger.LogEngineStats.InfoFormat("Start Time = {0}", DateTime.Now.ToLongDateString());

            //read config
            var readConnection = ConfigurationManager.ConnectionStrings["DebugConnection"].ConnectionString;
            var writeConnection = ConfigurationManager.ConnectionStrings["Write"].ConnectionString;
            //var procLog = ConfigurationManager.ConnectionStrings["ProcLog"].ConnectionString;

            bool shouldProcessPageThresholds = bool.Parse(ConfigurationManager.AppSettings["PageThesholdFlag"]);
            bool shouldProcessTimeouts = bool.Parse(ConfigurationManager.AppSettings["TimeoutsFlag"]);
            bool shouldProcessEdhid = bool.Parse(ConfigurationManager.AppSettings["EDHIDFlag"]);
            //bool shouldProcessMrSixProcLog = bool.Parse(ConfigurationManager.AppSettings["MrSixProcLogFlag"]);

            var numWorkerThreads = int.Parse(ConfigurationManager.AppSettings["NumWorkerThreads"]);
            Logger.LogEngineStats.InfoFormat("Read Connection = {0}", readConnection);
            Logger.LogEngineStats.InfoFormat("Write Connection = {0}", writeConnection);
            //Logger.LogEngineStats.InfoFormat("MrSix ProcLog Connection = {0}", procLog);

            Logger.LogEngineStats.InfoFormat("PageThesholdFlag = {0}", shouldProcessPageThresholds.ToString());
            Logger.LogEngineStats.InfoFormat("TimeoutsFlag = {0}", shouldProcessTimeouts.ToString());
            Logger.LogEngineStats.InfoFormat("EDHIDFlag = {0}", shouldProcessEdhid.ToString());
            //Logger.LogEngineStats.InfoFormat("MrSixProcLogFlag = {0}", shouldProcessMrSixProcLog.ToString());

            Logger.LogEngineStats.InfoFormat("NumWorkerThreads = {0}", numWorkerThreads.ToString(CultureInfo.InvariantCulture));

            if (shouldProcessPageThresholds)
            {
                var pgm = new PageThreshold.PageThreshold(numWorkerThreads, 500);
                pgm.Execute(readConnection, writeConnection);
            }

            if (shouldProcessTimeouts)
            {
                var pgm = new PageTimeout(numWorkerThreads, 500);
                pgm.Execute(readConnection, writeConnection);
            }

            if (shouldProcessEdhid)
            {
                var pgm = new PageEdhid(numWorkerThreads, 500);
                pgm.Execute(readConnection, writeConnection);
            }
        }

        static void Main()
        {
            try
            {
                Logger.Init();
                //Timeout.PageThresholdReader.Test("C:\\temp\\timeout\\timeout1.xml");
                Process();
            }
            catch(Exception ex)
            {
                Logger.LogExceptions.Error("Exception", ex);
            }
            finally
            {
                Logger.LogEngineStats.InfoFormat("End Time = {0}", DateTime.Now.ToLongDateString());
            }
        }


    }
}
