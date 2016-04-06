using System;
using System.Threading.Tasks;
using ThresholdAnalysis.Utils;
using ThresholdAnalysis.utils;

namespace ThresholdAnalysis.Timeout
{
    public class PageTimeout : IPageProcessor
    {
        private const string MaxLogEntryDateQueryString =
            @"select   max(LogEntryDateTime),  max(SeqNum)  from [dbo].[PageTimeout](nolock)";


        private const string MaxLogEntrySequenceQueryString =
                        @"select  max(LogEntryId)  from [dbo].[PageTimeout](nolock) where LogEntryDateTime > '2013-02-21 09:56:00'";



        private TaskProcessor _processor;
        private PersistTimeout _persistTimeout;
        private readonly int _maxThreads;
        private readonly int _queueSize;



        private const int MaxRetries = 5;

        public PageTimeout(int maxThreads, int queueSize)
        {
            _maxThreads = maxThreads;
            _queueSize = queueSize;
        }


        private void Start(string readConnection, string writeConnection,
                        DateTime start, int nextSeqNum, int maxLogSeq)
        {
            _persistTimeout = new PersistTimeout(writeConnection);

            _processor = new TaskProcessor(_maxThreads, _queueSize);

            _processor.Start();


            var task = new Task(() => Util.PrintQueueLength(_processor, 180000)); //print every 3 minutes
            task.Start();

            try
            {
                var pf = new PageProducer(nextSeqNum, maxLogSeq,  PageProducer.TimeoutMessageText);
                pf.Produce(readConnection, start, this);
            }
            finally
            {
                _processor.ShutDown();
                _persistTimeout.Flush();
            }
        }

        public void Process(BasePage basePage, string messageDetails)
        {
            _processor.Enqueue(new PageTimeoutReader(new TimeoutPage(basePage), messageDetails, _persistTimeout));
        }


        public void Execute(string readConnection, string writeConnection)
        {
            ProcessPageWithRetry(readConnection, writeConnection);
            PersistSummary(writeConnection);
        }

        private void ProcessPageWithRetry(string readConnection, string writeConnection)
        {
            int numAttempts = 0;

            while (numAttempts < MaxRetries)
            {
                try
                {
                    ProcessPage(readConnection, writeConnection);
                    return;
                }
                catch (Exception e)
                {
                    numAttempts++;

                    Logger.LogExceptions.Error("Exception encountered", e);

                    if (numAttempts >= MaxRetries)
                        throw;

                }
            }
        }

        private void ProcessPage(string readConnection, string writeConnection)
        {
            int nextSeqNum;
            var maxLogEntryDt = Util.GetMaxProcessedDate(writeConnection, MaxLogEntryDateQueryString, out nextSeqNum );
            var maxLogSeq = Util.GetMaxProcessedLogSequence(writeConnection, MaxLogEntrySequenceQueryString);

            Logger.LogEngineStats.InfoFormat("Starting Page Timeout at {0}:  Start Date={1},  NextSeqNum={2}", DateTime.Now, maxLogEntryDt, nextSeqNum);
            Start(readConnection, writeConnection, maxLogEntryDt, nextSeqNum, maxLogSeq);
            Logger.LogEngineStats.InfoFormat("Finished Page Timeout at {0}", DateTime.Now);
        }

        private static void PersistSummary(string writeConnection)
        {
            Logger.LogEngineStats.InfoFormat("Starting summary collection at {0}: ", DateTime.Now);

            var persist = new PageTimeoutHeaderSummary(writeConnection, 0);
            //Console.WriteLine(persistThreshold.GetCreateTableDdl());
            //Console.Read();
            persist.WriteSummary(false);
            Logger.LogEngineStats.InfoFormat("Finished summary collection at {0}", DateTime.Now);
        }

    }
}
