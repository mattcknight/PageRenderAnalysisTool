using System;
using ThresholdAnalysis.Utils;

namespace ThresholdAnalysis.EDHID
{
    public class PageEdhidPage : BasePage
    {
        public string DataSourceName { get; set; }
        public string Details { get; set; }
        public string MessageText { get; set; }

        public PageEdhidPage(BasePage basePage) : base(basePage){}

        public PageEdhidPage(int seqNum, DateTime logEntryDate, string pageText) : base(seqNum, logEntryDate, pageText){}
    }
}