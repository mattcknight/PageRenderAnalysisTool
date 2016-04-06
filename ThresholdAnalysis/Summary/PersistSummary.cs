using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using ThresholdAnalysis.Utils;

namespace ThresholdAnalysis.Summary
{
    public abstract class PersistSummary
    {
        public abstract string TableName { get; }
        public abstract String[] ColumnNames { get; }
        public abstract int NumKeys { get; }
        public abstract int NumMeasures { get; }
        public int NWeeks { get; set; } 

        public string WriteConnection { get; set; }
        public const string ReportStartDateQuery = @"select Max({0}) from {1} (nolock)";
        public const string SummaryRecordsQuery = @"select {0} from {1} (nolock) where {2} > @StartDt";

        public static DateTime NullDtmValue = new DateTime(1970, 1, 1);

        public DateTime GetMaxLogDate()
        {
            using (var conn = new SqlConnection(WriteConnection))
            {
                conn.Open();
                var cmdString = string.Format(ReportStartDateQuery, ColumnNames[0], TableName);
                using (var cmd = new SqlCommand(cmdString, conn))
                {
                    cmd.CommandTimeout = 0;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return SqlReaderUtil.GetDateTime(reader, 0, NullDtmValue);
                        }
                    }
                }
            }
            return NullDtmValue;
        }

        public abstract SqlCommand GetSummaryQuery(DateTime maxLogDate);

        public List<SummaryRecord> GetSummaryFromLeaf(DateTime maxLogDate)
        {
            var rtnValue = new List<SummaryRecord>();
            using (var connection = new SqlConnection(WriteConnection))
            {
                connection.Open();
                using (var cmd = GetSummaryQuery(maxLogDate))
                {
                    cmd.Connection = connection;
                    cmd.CommandTimeout = 0;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var record = SummaryRecord.Read(reader, NumKeys, NumMeasures);
                            rtnValue.Add(record);
                        }
                    }
                }
            }
            return rtnValue;
        }


        public string GetCreateTableDdl()
        {
            var builder = new StringBuilder();

            builder.Append(@"Create Table ").Append(TableName).Append("(\r\n");
            builder.Append(ColumnNames[0]).Append(" [smalldatetime] NOT NULL").Append("\r\n");

            for (int i = 0; i < NumKeys; i++)
            {
                builder.Append(",").Append(ColumnNames[1 + i]).Append(" [int] NOT NULL").Append("\r\n");
            }

            for (int i = 0; i < NumMeasures; i++)
            {
                builder.Append(",");
                builder.Append(ColumnNames[1 + NumKeys + i]).Append(" [bigint] NOT NULL").Append("\r\n");

            }

            for (int i = 0; i < NumMeasures; i++)
            {
                builder.Append(",");
                builder.Append("Yesterday_" + ColumnNames[1 + NumKeys + i]).Append(" [bigint] NOT NULL").Append("\r\n");

            }

            for (int i = 0; i < NumMeasures; i++)
            {
                builder.Append(",");
                builder.Append("WoW_" + ColumnNames[1 + NumKeys + i]).Append(" [bigint] NOT NULL").Append("\r\n");

            }


            for (int i = 0; i < NumMeasures; i++)
            {
                builder.Append(",");
                builder.Append("NWoW_" + ColumnNames[1 + NumKeys + i]).Append(" [bigint] NOT NULL").Append("\r\n");

            }

            builder.Append(")");

            return builder.ToString();

        }


        protected List<SummaryRecord> ComputeSummaryRecords()
        {
            var maxLogDate = GetMaxLogDate();

            var records = GetSummaryFromLeaf(maxLogDate);

            List<SummaryRecord> existingRecords;
            if (maxLogDate != NullDtmValue)
            {
                DateTime nWeeksBefore = maxLogDate.AddDays(-7 * (NWeeks + 1));
                existingRecords = ReadHistoryRecords(nWeeksBefore);
            }
            else
            {
                existingRecords = new List<SummaryRecord>();
            }

            StatMetrics.Compute(records, existingRecords, NWeeks);

            return records;
        }

        //candidate for a utility function
        private string GetColumnsString()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < ColumnNames.Length; i++)
            {
                if (i != 0)
                    builder.Append(",");
                builder.Append(ColumnNames[i]);
            }
            return builder.ToString();
        }


        protected List<SummaryRecord> ReadHistoryRecords(DateTime startDt)
        {
            var rtnValue = new List<SummaryRecord>();
            var query = string.Format(SummaryRecordsQuery, GetColumnsString(), TableName, ColumnNames[0]);
            using (var connection = new SqlConnection(WriteConnection))
            {
                connection.Open();
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@StartDt", startDt);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var record = SummaryRecord.Read(reader, NumKeys, NumMeasures);
                            rtnValue.Add(record);
                        }
                    }
                }
            }
            return rtnValue;
        }


        private DataTable CreateDataTable(bool bShowExtendedMeasures = true)
        {
            var headerTable = new DataTable(TableName);

            Type dateType = Type.GetType("System.DateTime");
            Type intType = Type.GetType("System.Int32");
            Type longType = Type.GetType("System.Int64");
            Type byteType = Type.GetType("System.Byte");

            var dc = new DataColumn { ColumnName = ColumnNames[0], DataType = dateType };
            headerTable.Columns.Add(dc);

            for (int i = 0; i < NumKeys; i++ )
            {
                dc = new DataColumn { ColumnName = ColumnNames[1 + i], DataType = intType };
                headerTable.Columns.Add(dc);
            }

            dc = new DataColumn { ColumnName = "DC", DataType = byteType };
            headerTable.Columns.Add(dc);

            for (int i = 0; i < NumMeasures; i++)
            {
                dc = new DataColumn { ColumnName = ColumnNames[ 2 + NumKeys + i], DataType = longType };
                headerTable.Columns.Add(dc);
            }

            if (bShowExtendedMeasures)
            {
                for (int i = 0; i < NumMeasures; i++)
                {
                    dc = new DataColumn { ColumnName = "PrevHr_" + ColumnNames[2 + NumKeys + i], DataType = longType };
                    headerTable.Columns.Add(dc);
                }

                for (int i = 0; i < NumMeasures; i++)
                {
                    dc = new DataColumn { ColumnName = "Yesterday_" + ColumnNames[2 + NumKeys + i], DataType = longType };
                    headerTable.Columns.Add(dc);
                }

                for (int i = 0; i < NumMeasures; i++)
                {
                    dc = new DataColumn { ColumnName = "WoW_" + ColumnNames[2 + NumKeys + i], DataType = longType };
                    headerTable.Columns.Add(dc);
                }

                for (int i = 0; i < NumMeasures; i++)
                {
                    dc = new DataColumn { ColumnName = "NWoW_" + ColumnNames[2 + NumKeys + i], DataType = longType };
                    headerTable.Columns.Add(dc);
                }
            }

            return headerTable;
        }

        protected void WriteSummary(List<SummaryRecord> records, bool bShowExtendedMeasures = true)
        {
            var headerTable = CreateDataTable(bShowExtendedMeasures);

            foreach (var record in records)
            {
                DataRow dr = headerTable.NewRow();
                int index = 0;
                dr[index++] = record.LogDate;

                int numKeys = record.Key.Values.Length;
                for (int j = 0; j < numKeys; j++ )
                {
                    dr[index++] = record.Key.Values[j];
                }

                dr[index++] = record.DataCenter;

                int numMeasures = record.Measures.Length;

                for (int j = 0; j < numMeasures; j++)
                {
                    dr[index++] = record.Measures[j];
                }

                if(bShowExtendedMeasures)
                {
                    for (int j = 0; j < numMeasures; j++)
                    {
                        if (record.PrevHourMeasures[j] == null)
                            dr[index++] = DBNull.Value;
                        else
                            dr[index++] = record.PrevHourMeasures[j];
                    }

                    for (int j = 0; j < numMeasures; j++)
                    {
                        if (record.YesterdayMeasures[j] == null)
                            dr[index++] = DBNull.Value;
                        else
                            dr[index++] = record.YesterdayMeasures[j];
                    }

                    for (int j = 0; j < numMeasures; j++)
                    {
                        if (record.Wo1WMeasures[j] == null)
                            dr[index++] = DBNull.Value;
                        else
                            dr[index++] = record.Wo1WMeasures[j];
                    }

                    for (int j = 0; j < numMeasures; j++)
                    {
                        if (record.WonWMeasures[j] == null)
                            dr[index++] = DBNull.Value;
                        else
                            dr[index++] = record.WonWMeasures[j];
                    }
                }


                headerTable.Rows.Add(dr);
            }

            if (headerTable.Rows.Count > 0)
            {
                using (var headerbulkInsert = new SqlBulkCopy(WriteConnection))
                {
                    headerbulkInsert.BatchSize = headerTable.Rows.Count;
                    headerbulkInsert.DestinationTableName = TableName;

                    headerbulkInsert.WriteToServer(headerTable);
                }
            }
        }

        public void WriteSummary(bool bShowExtendedMeasures = true)
        {
            var records = ComputeSummaryRecords();
            WriteSummary(records, bShowExtendedMeasures);
        }
    }
}
