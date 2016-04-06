using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using ThresholdAnalysis.Utils;

namespace ThresholdAnalysis.Timeout
{
    public class PersistTimeout
    {
        private const string DestinationHdr = "PageTimeout";
        private const string DataSourceAliasTbl = "PageThresholdDataSourceAlias";
        private const string StackTraceTbl = "PageStackTrace";
        private const string MsgDetailsTbl = "PageMessageDetails";

        private static readonly Type IntType = Type.GetType("System.Int32");
        private static readonly Type BoolType = Type.GetType("System.Boolean");



 
        private const int FlushSize = 1000;
        private List<TimeoutPage> _pages = new List<TimeoutPage>(1000);
        private readonly string _connectionString;

        private static readonly IDictionary<string, int> DataSourceAliasMap = new Dictionary<string, int>();
        private static readonly IDictionary<string, int> StackTraceMap = new Dictionary<string, int>();
        private static readonly IDictionary<string, int> MsgDetailsMap = new Dictionary<string, int>();



        public PersistTimeout(string connectionString)
        {
            _connectionString = connectionString;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(TimeoutPage pageThresholdPage)
        {
            _pages.Add(pageThresholdPage);
            if (_pages.Count > FlushSize)
            {
                Console.WriteLine("Flushing Pages to disk. Current SeqNum = {0}", pageThresholdPage.SeqNum);

                BulkInsert(_connectionString, _pages);
                _pages = new List<TimeoutPage>(1000);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Flush()
        {
            if (_pages.Count > 0)
            {
                BulkInsert(_connectionString, _pages);
                _pages = new List<TimeoutPage>(1000);
            }
        }

        private static void BulkInsertHeader(SqlConnection connection, IEnumerable<TimeoutPage> pages)
        {

            var headerTable = new DataTable(DestinationHdr);

            /* SeqNum, LogEntryDate, PageCode, ExecutionTime, DbTime */



            var dc = new DataColumn { ColumnName = "SeqNum", DataType = IntType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "LogEntryDateTime", DataType = Type.GetType("System.DateTime") };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "LogEntryId", DataType = IntType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "WebServer", DataType = Type.GetType("System.String") };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "UserId", DataType = IntType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "PageCode", DataType = IntType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "Sid", DataType = Type.GetType("System.Guid") };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "IsSubscriber", DataType = BoolType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "TimeoutDataSourceAliasId", DataType = IntType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn {ColumnName = "StackTraceId", DataType = IntType};
            headerTable.Columns.Add(dc);

            dc = new DataColumn {ColumnName = "MsgDetailsId", DataType = IntType};
            headerTable.Columns.Add(dc);


            var dsTable = CreateDataSourceTable();
            var stackTraceTable = CreateStackTraceTable();
            var msgDetailsTable = CreateMessageDetailsTable();



            foreach (var page in pages)
            {
                int dsId = Util.GetId(DataSourceAliasMap, page.DataSourceName, dsTable);
                int stackTraceId = Util.GetId(StackTraceMap, page.TraceData, stackTraceTable);
                int msgDetailsId = Util.GetId(MsgDetailsMap, page.Details, msgDetailsTable);

                DataRow dr = headerTable.NewRow();
                dr[0] = page.SeqNum;
                dr[1] = page.LogEntryDate;
                dr[2] = page.LogEntryId;

                dr[3] = page.WebServer;

                dr[4] = page.UserId;
                dr[5] = page.PageCode ?? -1;

                dr[6] = page.Sid;
                dr[7] = page.IsSubscriber;

                dr[8] = dsId;
                dr[9] = stackTraceId;
                dr[10] = msgDetailsId;

                headerTable.Rows.Add(dr);
            }

            WriteTable(connection, dsTable);
            WriteTable(connection, stackTraceTable);
            WriteTable(connection, msgDetailsTable);

            WriteTable(connection, headerTable);

        }

        private static DataTable CreateStackTraceTable()
        {
            var stackTraceTable = new DataTable(StackTraceTbl);
            var dc = new DataColumn { ColumnName = "StackTraceId", DataType = IntType };
            stackTraceTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "StackTrace", DataType = Type.GetType("System.String") };
            stackTraceTable.Columns.Add(dc);

            return stackTraceTable;
        }

        private static DataTable CreateDataSourceTable()
        {
            var dsTable = new DataTable(DataSourceAliasTbl);
            var dc = new DataColumn { ColumnName = "DataSourceAliasId", DataType = IntType };
            dsTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "DataSourceAlias", DataType = Type.GetType("System.String") };
            dsTable.Columns.Add(dc);

            return dsTable;
        }

        private static DataTable CreateMessageDetailsTable()
        {
            var dsTable = new DataTable(MsgDetailsTbl);
            var dc = new DataColumn { ColumnName = "MessageDetailsId", DataType = IntType };
            dsTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "MessageDetails", DataType = Type.GetType("System.String") };
            dsTable.Columns.Add(dc);

            return dsTable;
        }

        private static void WriteTable(SqlConnection connection, DataTable table)
        {
            if (table.Rows.Count > 0)
            {
                using (var tblBulkInsert = new SqlBulkCopy(connection))
                {
                    tblBulkInsert.BatchSize = table.Rows.Count;
                    tblBulkInsert.DestinationTableName = table.TableName;
                    tblBulkInsert.WriteToServer(table);
                }

            }
        }


        private static void BulkInsert(String connectionString, IEnumerable<TimeoutPage> pages)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                TryPopulateLookUps(connection);
                BulkInsertHeader(connection, pages);
            }

        }

        private static void TryPopulateLookUps(SqlConnection connection)
        {
            if (DataSourceAliasMap.Count == 0)
            {
                Util.PopulateLookup(connection, DataSourceAliasMap, DataSourceAliasTbl);
                Util.PopulateLookup(connection, StackTraceMap, StackTraceTbl);
                Util.PopulateLookup(connection, MsgDetailsMap, MsgDetailsTbl);
            }

        }

    }
}
