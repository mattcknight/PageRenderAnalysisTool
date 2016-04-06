using ThresholdAnalysis.Summary;

namespace ThresholdAnalysis.PageThreshold
{
    public class PersistPageHeaderSummary : PersistHourlySummary
    {
        private static readonly string[] ColumnNames2 =
        {
            "LogEntryDate",
            "HourOfDay",
            "PageId",
            "DC",
            "ExecutionTime",
            "DbTime",
            "CntExecutions"
        };

        private const string QueryLeaf =
            @"
   set transaction isolation level read uncommitted
   Select cast(LogEntryDateTime as date), 
   cast(datepart(hour, LogEntryDateTime) as tinyint),  
   PageId, 
   DC = cast((case when WebServer like 'DA%' then 1 else 2 end) as tinyint) ,
   cast( AVG(ExecutionTime) as bigint), 
   cast(AVG(DbTime) as bigint), 
   cast(Count(1) as bigint)
   from PageThresholdHeader 
   where LogEntryDateTime >= @StartDt and LogEntryDateTime < @EndDt and ExecutionTime > 5000
   Group By
   cast(LogEntryDateTime as date), 
   cast(datepart(hour, LogEntryDateTime) as tinyint), 
   PageId,
   cast((case when WebServer like 'DA%' then 1 else 2 end) as tinyint)
   Having Count(1) > 50 
   Order By 
   cast(LogEntryDateTime as date), 
   cast(datepart(hour, LogEntryDateTime) as tinyint),
   PageId,
   cast((case when WebServer like 'DA%' then 1 else 2 end) as tinyint)

";
        public PersistPageHeaderSummary(string writeConnection, int nWeeks)
        {
            WriteConnection = writeConnection;
            NWeeks = nWeeks;
        }

        public override string TableName
        {
            get { return "PageThresholdHeaderHourlySummary"; }
        }

        public override string[] ColumnNames
        {
            get { return ColumnNames2; }
        }

        public override int NumKeys
        {
            get { return 2; }
        }

        public override int NumMeasures
        {
            get { return 3; }
        }

 
        public override string GetSummaryQuery()
        {
            return QueryLeaf;
        }

 
    }
}
