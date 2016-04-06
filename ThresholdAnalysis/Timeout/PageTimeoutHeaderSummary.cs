using ThresholdAnalysis.Summary;

namespace ThresholdAnalysis.Timeout
{
    public class PageTimeoutHeaderSummary : PersistHourlySummary
    {
        private static readonly string[] ColumnNames2 =
        {
            "LogEntryDate",
            "HourOfDay",
            "TimeOutDataSourceAliasId",
            "DC",
            "CntTimeOuts"
        };

        private const string QueryLeaf =
            @"
   set transaction isolation level read uncommitted
   Select 
   cast(LogEntryDateTime as date), 
   cast(datepart(hour, LogEntryDateTime) as tinyint), 
   cast(TimeOutDataSourceAliasId as int), 
   cast(case when WebServer like 'DA%' then 1 else 2 end as tinyint) ,
   cast(count(1) as bigInt) as CntTimeOuts
   From [dbo].[PageTimeOut]
   where
   LogEntryDateTime >= @StartDt and LogEntryDateTime < @EndDt 
   Group by 
   cast(LogEntryDateTime as date), 
   cast(datepart(hour, LogEntryDateTime) as tinyInt), 
   cast(TimeOutdataSourceAliasId as int),
   cast(case when WebServer like 'DA%' then 1 else 2 end as tinyint)
Order by 
   cast(LogEntryDateTime as date), 
   cast(datepart(hour, LogEntryDateTime) as tinyInt), 
   cast(TimeOutdataSourceAliasId as int),
   cast(case when WebServer like 'DA%' then 1 else 2 end as tinyint)
";

        public PageTimeoutHeaderSummary(string writeConnection, int nWeeks)
        {
            WriteConnection = writeConnection;
            NWeeks = nWeeks;
        }

        public override string TableName
        {
            get { return "PageTimeoutHourlySummary"; }
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
            get { return 1; }
        }


        public override string GetSummaryQuery()
        {
            return QueryLeaf;
        }

    }
}
