using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using ThresholdAnalysis.Utils;

namespace ThresholdAnalysis.EDHID
{                 
    public class PersistEdhid
    {
        private const string DestionationHdr = "PageEdhid";
        private const string StackTraceTbl = "PageEDHIDStackTrace";
        private const string MsgTextTbl = "PageEDHIDMessageText";
        private const int FlushSize = 1000;

        private static readonly Type IntType = Type.GetType("System.Int32");
        private static readonly Type BoolType = Type.GetType("System.Boolean");
        
        private List<PageEdhidPage> _pages = new List<PageEdhidPage>(FlushSize);
        private readonly string _connectionString;

        private static readonly IDictionary<string, int> StackTraceMap = new Dictionary<string, int>();
        private static readonly IDictionary<string, int> MsgTextMap = new Dictionary<string, int>();

        public PersistEdhid(string connectionString)
        {
            _connectionString = connectionString;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(PageEdhidPage edhidPage)
        {
            _pages.Add(edhidPage);

            if (_pages.Count > FlushSize)
            {
                Console.WriteLine("Flushing Pages to disck.  Current SeqNum = {0}", edhidPage.SeqNum);
                BulkInsert(_connectionString, _pages);
                _pages = new List<PageEdhidPage>(FlushSize);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Flush()
        {
            if (_pages.Count > 0)
            {
                BulkInsert(_connectionString, _pages);
                _pages = new List<PageEdhidPage>(FlushSize);
            }
        }

        private static void BulkInsertHeader(SqlConnection connection, IEnumerable<PageEdhidPage> pages)
        {
            var headerTable = new DataTable(DestionationHdr);

            /*SeqNum, LogEntryDateTime, LogEntryID, WebServer, UserID, PageCode, IsSubscriber, StackTrace, MessageText*/

            var dc = new DataColumn { ColumnName = "SeqNum", DataType = IntType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "LogEntryDateTime", DataType = Type.GetType("System.DateTime") };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "LogEntryId", DataType = IntType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "WebServer", DataType = Type.GetType("System.String") };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "UserID", DataType = IntType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "PageCode", DataType = IntType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "Sid", DataType = Type.GetType("System.Guid") };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "IsSubscriber", DataType = BoolType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "StackTraceId", DataType = IntType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "MessageTextId", DataType = IntType };
            headerTable.Columns.Add(dc);

            var stackTraceTable = CreateStackTraceTable();
            var msgTextTable = CreateMessageTextTable();

            foreach (var page in pages)
            {
                int stackTraceId = Util.GetId(StackTraceMap, page.TraceData, stackTraceTable);
                int msgTextId = Util.GetId(MsgTextMap, page.MessageText, msgTextTable);

                DataRow dr = headerTable.NewRow();
                dr[0] = page.SeqNum;
                dr[1] = page.LogEntryDate;
                dr[2] = page.LogEntryId;
                dr[3] = page.WebServer;
                dr[4] = page.UserId;
                if (page.PageCode == null)
                    dr[5] = DBNull.Value;
                else
                    dr[5] = page.PageCode;
                dr[6] = page.Sid;
                dr[7] = page.IsSubscriber;
                dr[8] = stackTraceId;
                dr[9] = msgTextId;

                headerTable.Rows.Add(dr);
            }

            WriteTable(connection, stackTraceTable);
            WriteTable(connection, msgTextTable);
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

        private static DataTable CreateMessageTextTable()
        {
            var messageTextTable = new DataTable(MsgTextTbl);

            var dc = new DataColumn { ColumnName = "MessageTextId", DataType = IntType };
            messageTextTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "MessageText", DataType = Type.GetType("System.String") };
            messageTextTable.Columns.Add(dc);

            return messageTextTable;
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

        private static void BulkInsert(String connectionString, IEnumerable<PageEdhidPage> pages)
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
            Util.PopulateLookup(connection, StackTraceMap, StackTraceTbl);
            Util.PopulateLookup(connection, MsgTextMap, MsgTextTbl);
        }


    }
}