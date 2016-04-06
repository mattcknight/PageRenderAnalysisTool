using ThresholdAnalysis.Summary;

namespace ThresholdAnalysis.PageThreshold
{
    public class PersistProcThresholdSummary : PersistHourlySummary
    {
        private static readonly string[] ColumnNames2 =
        {
            "LogEntryDate",
            "HourOfDay",
            "ProcNameId",
            "DataSourceAliasId",
            "DC",
            "CntExecutions"
        };

        private const string QueryLeaf =
            @"
set transaction isolation level read uncommitted
Select cast(ptd.LogEntryDateTime as date), 
   cast(datepart(hour, ptd.LogEntryDateTime) as tinyint), 
   cast(ptd.ProcNameId as int),
   cast(ptd.DataSourceAliasId as int),
   cast( case when pth.WebServer like 'DA%' then 1 else 2 end as tinyint),   
   cast(Count(1) as bigint) 
   from PageThresholdDetail ptd
   Inner Join dbo.PageThresholdHeader pth On pth.SeqNum = ptd.SeqNum
   Where 
   ptd.LogEntryDateTime >= @StartDt and ptd.LogEntryDateTime < @EndDt 
   and ptd.ExecutionTime > 5000 
   Group by 
   cast(ptd.LogEntryDateTime as date), 
   cast(datepart(hour, ptd.LogEntryDateTime) as tinyint),
   cast(ptd.ProcNameId as int), 
   cast(ptd.DataSourceAliasId as int),
   cast( case when pth.WebServer like 'DA%' then 1 else 2 end as tinyint)
   Order By 
   cast(ptd.LogEntryDateTime as date), 
   cast(datepart(hour, ptd.LogEntryDateTime) as tinyint),
   cast(ptd.ProcNameId as int), 
   cast(ptd.DataSourceAliasId as int),
   cast( case when pth.WebServer like 'DA%' then 1 else 2 end as tinyint)
"; 

        public PersistProcThresholdSummary(string writeConnection, int nWeeks)
        {
            WriteConnection = writeConnection;
            NWeeks = nWeeks;
        }

        public override string TableName
        {
            get { return "ProcThresholdHourlySummary"; }
        }

        public override string[] ColumnNames
        {
            get { return ColumnNames2; }
        }

        public override int NumKeys
        {
            get { return 3; }
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
