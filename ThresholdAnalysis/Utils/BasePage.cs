using System;

namespace ThresholdAnalysis.Utils
{
    public class BasePage
    {
        public string PageTxt { get; set; }
        public int SeqNum { get; set; }
        public string WebServer { get; set; }
        public int LogEntryId { get; set; }
        public int UserId;
        public Guid Sid { get; set; }
        public bool IsSubscriber { get; set; }
        public DateTime LogEntryDate;
        public int? PageCode { get; set; }
        public string TraceData { get; set; }


        public BasePage(int seqNum, DateTime logEntryDate, string pageText)
        {
            LogEntryDate = logEntryDate;
            SeqNum = seqNum;
            PageTxt = pageText;
        }

        public BasePage(BasePage basePage)
        {
            LogEntryDate = basePage.LogEntryDate;
            SeqNum = basePage.SeqNum;
            PageTxt = basePage.PageTxt;
            WebServer = basePage.WebServer;
            LogEntryId = basePage.LogEntryId;
            UserId = basePage.UserId;
            Sid = basePage.Sid;
            IsSubscriber = basePage.IsSubscriber;
            PageCode = basePage.PageCode;
            TraceData = basePage.TraceData;
        }

    }
}
