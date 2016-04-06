using System;
using System.Threading.Tasks;
using ThresholdAnalysis.Utils;
using ThresholdAnalysis.utils;

namespace ThresholdAnalysis.EDHID
{
    public class PageEdhid : IPageProcessor
    {
        private const string MaxLogEntryDateQueryString = @"select max(LogEntryDateTime),  max(SeqNum)  from [dbo].[PageEDHID](nolock) ";
        private const string MaxLogEntrySequenceQueryString =
                        @"select  max(LogEntryId)  from [dbo].[PageEDHID](nolock) where LogEntryDateTime > '2013-02-21 09:56:00'";

        private TaskProcessor _processor;
        private PersistEdhid _persistEdhid;
        private readonly int _maxThreads;
        private readonly int _queueSize;
        private const int MaxRetries = 5;

        public PageEdhid(int maxThreads, int queueSize)
        {
            _maxThreads = maxThreads;
            _queueSize = queueSize;            
        }

        private void Start(string readConnection, string writeConnection, DateTime start, int nextSeqNum, int maxLogSeq)
        {
            _persistEdhid = new PersistEdhid(writeConnection);
            _processor = new TaskProcessor(_maxThreads, _queueSize);
            _processor.Start();

            var task = new Task(() => Util.PrintQueueLength(_processor, 180000));
            task.Start();

            try
            {
                    var pf = new PageProducer(nextSeqNum, maxLogSeq, "%", "17");
                    pf.Produce(readConnection, start, this);
            }
            finally
            {
                _processor.ShutDown();
                _persistEdhid.Flush();
            }
        }

        public void Process(BasePage basePage, string messageDetails)
        {
            _processor.Enqueue( new PageEdhidReader(new PageEdhidPage(basePage), messageDetails, _persistEdhid));
        }

        public void Execute(string readConnection, string writeConnection)
        {
            ProcessPageWithRetry(readConnection, writeConnection);
        }

        public void ProcessPageWithRetry(string readConnection, string writeConnection)
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

        public void ProcessPage(string readConnection, string writeConnection)
        {
            int nextSeqNum;
            var maxLogEntryDt = Util.GetMaxProcessedDate(writeConnection, MaxLogEntryDateQueryString, out nextSeqNum);
            var maxLogSeq = Util.GetMaxProcessedLogSequence(writeConnection, MaxLogEntrySequenceQueryString);
            Logger.LogEngineStats.InfoFormat("Starting Page EDHID at {0}: Start Date = {1}, NextSeqNum={2}", DateTime.Now, maxLogEntryDt, nextSeqNum);
            Start(readConnection, writeConnection, maxLogEntryDt, nextSeqNum, maxLogSeq);
            Logger.LogEngineStats.InfoFormat("Finished Page EDHID at {0}", DateTime.Now);
        }

    }
}