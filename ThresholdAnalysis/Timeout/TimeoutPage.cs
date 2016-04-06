using System;
using ThresholdAnalysis.Utils;

namespace ThresholdAnalysis.Timeout
{
    public class TimeoutPage : BasePage
    {
        public string DataSourceName { get; set; }
        public string Details { get; set; }

        public TimeoutPage(BasePage basePage)
            : base(basePage)
        {
            
        }
        
        public TimeoutPage(int seqNum, DateTime logEntryDate, string pageText) : base(seqNum, logEntryDate, pageText)
        {
        }

    }
}
