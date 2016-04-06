using System;
using System.Collections.Generic;
using ThresholdAnalysis.Utils;

namespace ThresholdAnalysis.PageThreshold
{
    public class PageThresholdPage : BasePage
    {
        public static readonly string PageThresholdMessageTextSample = "Page render threshold (00:00:05) exceeded%";

        public PageThresholdPage(BasePage basePage)
            : base(basePage)
        {
            PageTxt = PageTxt.Substring(PageThresholdMessageTextSample.Length - 1);
        }

        public long ExecutionTime { get; set; }

        public int PageId { get; set; }

        public bool HasLoginAuth { get; set; }

        public long DbTime
        {
            get 
            { 
                long total = 0;
                foreach (var dbItem in DbItems)
                {
                    total += dbItem.ExecutionTime;
                }
                return total;
            }
        }

        public long ComputedDbTime
        {
            get
            {
                long total = 0;
                foreach (var dbItem in DbItems)
                {
                    total += dbItem.ComputedExecutionTime;
                }
                return total;
            }            
        }
        
        public List<DbCall> DbItems = new List<DbCall>();

        public void AddDbItem( DbCall call)
        {
            DbItems.Add(call);        
        }

        public string Request { get; set; }

    }

    public class DbCall
    {
        public string ProcName;
        public string DataSourceAlias;
        public long ExecutionTime;
        public long ComputedExecutionTime;
        public DateTime Begin;
        public DateTime End;
        public int ProcSeq;
        public string ServerName;
        public long? ServerDuration;
        
        public DbCall(string procName, string datasourceAlias, long executionTime)
        {
            ProcName = procName;
            ExecutionTime = executionTime;
            DataSourceAlias = datasourceAlias ?? "Unknown";
        }

        public bool IdentifiesUserId()
        {
            return ProcName.Equals("[DoubleBlind].[dbo].[dbFistBagGet]") ||
                   ProcName.Equals("[ProfileReadCode].[dbo].[Sharkfin]");
        }

        public bool IsLoginAuth()
        {
            return ProcName.Equals("[Starter].[dbo].[LoginAuth]");
        }


 
    }
}
