using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ThresholdAnalysis.MrSix
{
    public class DataWriter
    {
        private readonly string _connection;
        public  DataWriter(string connection)
        {
            _connection = connection;
        }

        public void Write(IEnumerable<SearchSummary> searchSummary, DateTime date)
        {
            var seachSummaryTable = new DataTable("MrSixSearchSummary");

            /* SeqNum, LogEntryDate, PageCode, ExecutionTime, DbTime */
            Type intType = Type.GetType("System.Int32");
            Type stringType = Type.GetType("System.String");
            Type dateType = Type.GetType("System.DateTime");

            seachSummaryTable.Columns.Add(new DataColumn { ColumnName = "LogDay", DataType = dateType });
            seachSummaryTable.Columns.Add(new DataColumn { ColumnName = "SearchName", DataType = stringType });
            seachSummaryTable.Columns.Add(new DataColumn { ColumnName = "Total", DataType = intType });
            seachSummaryTable.Columns.Add(new DataColumn { ColumnName = "TotalLoggedIn", DataType = intType });
            seachSummaryTable.Columns.Add(new DataColumn { ColumnName = "TotalNonLoggedIn", DataType = intType });

            seachSummaryTable.Columns.Add(new DataColumn { ColumnName = "TotalZero", DataType = intType });
            seachSummaryTable.Columns.Add(new DataColumn { ColumnName = "TotalZeroLoggedIn", DataType = intType });
            seachSummaryTable.Columns.Add(new DataColumn { ColumnName = "TotalZeroNonLoggedIn", DataType = intType });


            foreach (var summary in searchSummary)
            {
                DataRow dr = seachSummaryTable.NewRow();
                dr[0] = date;
                dr[1] = summary.SrarchName;
                dr[2] = summary.Total;
                dr[3] = summary.TotalLoggedIn;
                dr[4] = summary.TotalNonLoggedIn;
                dr[5] = summary.TotalZero;
                dr[6] = summary.TotalZeroLoggedIn;
                dr[7] = summary.TotalZeroNonLoggedIn;
                seachSummaryTable.Rows.Add(dr);
            }



            if (seachSummaryTable.Rows.Count > 0)
            {
                using (var messageSourceBulkInsert = new SqlBulkCopy(_connection))
                {
                    messageSourceBulkInsert.BatchSize = seachSummaryTable.Rows.Count;
                    messageSourceBulkInsert.DestinationTableName = "MrSixSearchSummary";

                    messageSourceBulkInsert.WriteToServer(seachSummaryTable);
                }
            }
        }
    }
}
