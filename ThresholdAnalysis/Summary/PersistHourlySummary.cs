using System;
using System.Data.SqlClient;
using ThresholdAnalysis.Utils;

namespace ThresholdAnalysis.Summary
{
    public abstract class PersistHourlySummary : PersistSummary
    {
        public const string MaxReportHourQuery = @"select Max({0}) from {1} (nolock) where {2} = @MaxDate";

        private static DateTime GetReportEndDate()
        {
            var now = DateTime.Now.AddMinutes(-10);

            return new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        }

        public byte GetMaxSummaryHour(DateTime maxDate)
        {
            using (var conn = new SqlConnection(WriteConnection))
            {
                conn.Open();
                var cmdString = string.Format(MaxReportHourQuery, ColumnNames[1], TableName, ColumnNames[0]);
                using (var cmd = new SqlCommand(cmdString, conn))
                {
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@MaxDate", maxDate);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return SqlReaderUtil.GetByte(reader, 0, 0);
                        }
                    }
                }
            }
            return 0;

        }

        public abstract string GetSummaryQuery();

        public override SqlCommand GetSummaryQuery(DateTime maxLogDate)
        {
            byte currMaxHr = GetMaxSummaryHour(maxLogDate);

            var startDt = maxLogDate.AddHours(currMaxHr + 1);

            DateTime endDt = GetReportEndDate();

            var queryString = GetSummaryQuery();

            var cmd = new SqlCommand(queryString);

            cmd.Parameters.AddWithValue("@StartDt", startDt);
            cmd.Parameters.AddWithValue("@EndDt", endDt);

            return cmd;

        }


    }
}
