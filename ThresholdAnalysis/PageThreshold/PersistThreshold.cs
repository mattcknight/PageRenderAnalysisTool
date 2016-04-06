using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ThresholdAnalysis.PageThreshold
{
    class PersistThreshold 
    {
        private const string DestinationHdr = "PageThresholdHeader";
        private const string DestinationDtl = "PageThresholdDetail";

        //Lookup tables
        private const string DestinationPage = "PageThresholdPage";
        private const string DataSourceAliasTbl =  "PageThresholdDataSourceAlias";
        private const string ProcNameTbl = "PageThresholdProcName";
        private const string ServerNameTbl = "PageThresholdServerName";

        public static Type IntType = Type.GetType("System.Int32");
        public static Type LongType = Type.GetType("System.Int64");
        public static Type BoolType = Type.GetType("System.Boolean");
        public static Type StringType = Type.GetType("System.String");





        private const int FlushSize = 1000;
        private List<PageThresholdPage> _pages = new List<PageThresholdPage>(1000);
        private readonly string _connectionString;
        
        private static readonly IDictionary<string, int> Pages = new Dictionary<string, int>(); 
        private static readonly IDictionary<string, int> DataSourceAliasMap = new Dictionary<string, int>();
        private static readonly IDictionary<string, int> ProcNameMap = new Dictionary<string, int>();
        private static readonly IDictionary<string, int> ServerNameMap = new Dictionary<string, int>();


        public PersistThreshold(string connectionString)
        {
            _connectionString = connectionString;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]    
        public void Add(PageThresholdPage pageThresholdPage)
        {
            _pages.Add(pageThresholdPage);
            if (_pages.Count > FlushSize)
            {
                Console.WriteLine("Flushing Pages to disk. Current SeqNum = {0}", pageThresholdPage.SeqNum);

                BulkInsert(_connectionString, _pages);
                _pages = new List<PageThresholdPage>(1000);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]    
        public void Flush()
        {
            if (_pages.Count > 0)
            {
                BulkInsert(_connectionString, _pages);
                _pages = new List<PageThresholdPage>(1000);
            }
        }

        private static void BulkInsertHeader(SqlConnection connection, IEnumerable<PageThresholdPage> pages)
        {

            var headerTable = new DataTable(DestinationHdr);
            
            /* SeqNum, LogEntryDate, PageCode, ExecutionTime, DbTime */
 
            
            var dc = new DataColumn { ColumnName = "SeqNum", DataType = IntType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "PageId", DataType = IntType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "LogEntryDateTime", DataType = Type.GetType("System.DateTime") };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "LogEntryId", DataType = IntType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "WebServer", DataType = Type.GetType("System.String")};
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "UserId", DataType = IntType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "HasLoginAuth", DataType = BoolType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "Sid", DataType = Type.GetType("System.Guid") };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "IsSubscriber", DataType = BoolType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "ExecutionTime", DataType = LongType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "DbTime", DataType = LongType };
            headerTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "ComputedDbExecTime", DataType = LongType };
            headerTable.Columns.Add(dc);


            var pageTable = new DataTable(DestinationPage);
            dc = new DataColumn { ColumnName = "PageId", DataType = IntType };
            pageTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "PageThresholdPage", DataType = Type.GetType("System.String") };
            pageTable.Columns.Add(dc);

            

            foreach (var page in pages)
            {
                page.PageId = GetId(Pages, page.PageTxt, pageTable);
                DataRow dr = headerTable.NewRow();
                dr[0] = page.SeqNum;
                dr[1] = page.PageId;

                dr[2] = page.LogEntryDate;
                dr[3] = page.LogEntryId;

                dr[4] = page.WebServer;

                dr[5] = page.UserId;
                dr[6] = page.HasLoginAuth;

                dr[7] = page.Sid;
                dr[8] = page.IsSubscriber;

                dr[9] = page.ExecutionTime;
                dr[10] = page.DbTime;
                dr[11] = page.ComputedDbTime;
                headerTable.Rows.Add(dr);
            }

            WriteTable(connection, pageTable);

            WriteTable(connection, headerTable);

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

        private static int GetId(IDictionary<string, int> map, string text, DataTable dataTable)
        {
            if (!map.ContainsKey(text))
            {
                int max;
                if (map.Count == 0)
                {
                    max = 1;
                }
                else
                {
                    max = map.Values.Max() + 1;
                }

                map.Add(text, max);
                var row = dataTable.NewRow();
                row[0] = max;
                row[1] = text;
                dataTable.Rows.Add(row);
            }

            return map[text];
        }

        private static DataTable CreateDetailTable(string tblName)
        {
            var detailTable = new DataTable(tblName);

            /* SeqNum, PageId, ProcName, DataSourceAlias, ProcSeq,  ExecutionTime, BeginDt, EndDt,  ComputedExecTime, ServerNameId, Duration*/

            var dc = new DataColumn { ColumnName = "SeqNum", DataType = IntType };
            detailTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "ProcSeq", DataType = IntType };
            detailTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "ProcNameId", DataType = IntType };
            detailTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "DataSourceAliasId", DataType = IntType };
            detailTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "PageId", DataType = IntType };
            detailTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "LogEntryDateTime", DataType = Type.GetType("System.DateTime") };
            detailTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "ExecutionTime", DataType = LongType };
            detailTable.Columns.Add(dc);


            dc = new DataColumn { ColumnName = "BeginDate", DataType = Type.GetType("System.DateTime") };
            detailTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "EndDate", DataType = Type.GetType("System.DateTime") };
            detailTable.Columns.Add(dc);


            dc = new DataColumn { ColumnName = "ComputedExecTime", DataType = LongType };
            detailTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "ServerNameId", DataType = IntType};
            detailTable.Columns.Add(dc);


            dc = new DataColumn { ColumnName = "Duration", DataType = LongType };
            detailTable.Columns.Add(dc);


            return detailTable;

        }
      
        private static void BulkInsertDetail(SqlConnection connection, IEnumerable<PageThresholdPage> pages)
        {
            var detailTable = CreateDetailTable(DestinationDtl);

            var dsTable = new DataTable(DataSourceAliasTbl);
            var dc = new DataColumn { ColumnName = "DataSourceAliasId", DataType = IntType };
            dsTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "DataSourceAlias", DataType = StringType };
            dsTable.Columns.Add(dc);


            var procNameTable = new DataTable(ProcNameTbl);
            dc = new DataColumn { ColumnName = "ProcNameId", DataType = IntType };
            procNameTable.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "ProcName", DataType = StringType };
            procNameTable.Columns.Add(dc);

            var serverNameTbl = new DataTable(ServerNameTbl);
            dc = new DataColumn {ColumnName = "ServerNameId", DataType = IntType};
            serverNameTbl.Columns.Add(dc);

            dc = new DataColumn { ColumnName = "ServerName", DataType = StringType };
            serverNameTbl.Columns.Add(dc);



            foreach (var page in pages)
            {
                foreach(var dbCall in page.DbItems)
                {
                    DataRow dr = detailTable.NewRow();
                    int idx = 0;
                    dr[idx++] = page.SeqNum;
                    dr[idx++] = dbCall.ProcSeq;
                    dr[idx++] = GetId(ProcNameMap, dbCall.ProcName, procNameTable);
                    dr[idx++] = GetId(DataSourceAliasMap, dbCall.DataSourceAlias, dsTable);

                    dr[idx++] = page.PageId;
                    dr[idx++] = page.LogEntryDate;

                    dr[idx++] = dbCall.ExecutionTime;
                    dr[idx++] = dbCall.Begin;
                    dr[idx++] = dbCall.End;
                    dr[idx++] = dbCall.ComputedExecutionTime;

                    if (!string.IsNullOrEmpty(dbCall.ServerName))
                    {
                        dr[idx++] = GetId(ServerNameMap, dbCall.ServerName, serverNameTbl);
                    }
                    else
                    {
                        dr[idx++] = DBNull.Value;
                    }

                    if (dbCall.ServerDuration.HasValue)
                        dr[idx] = dbCall.ServerDuration.Value;
                    else
                    {
                        dr[idx] = DBNull.Value;
                    }
                    detailTable.Rows.Add(dr);
                }
            }


            WriteTable(connection, dsTable);
            WriteTable(connection, procNameTable);
            WriteTable(connection, detailTable);
            WriteTable(connection, serverNameTbl);
        }



        private static void BulkInsert(String connectionString, List<PageThresholdPage> pages)
        {
            using( var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                TryPopulateLookUps(connection);
                BulkInsertHeader(connection, pages);    
                BulkInsertDetail(connection, pages);
            }

        }

        private static void TryPopulateLookUps(SqlConnection connection)
        {
            if (Pages.Count == 0)
            {
                PopulateLookup(connection, Pages, DestinationPage);
                PopulateLookup(connection, DataSourceAliasMap, DataSourceAliasTbl);
                PopulateLookup(connection, ProcNameMap, ProcNameTbl);
                PopulateLookup(connection, ServerNameMap, ServerNameTbl);
            }

        }

        private static void PopulateLookup(SqlConnection connection, IDictionary<string, int> dictionary, string table)
        {
            using(var cmd = new SqlCommand("select * from " + table, connection))
            {
                cmd.CommandType = CommandType.Text;
                using (var reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        dictionary.Add(reader.GetString(1), reader.GetInt32(0));
                    }
                }
            }
        }
    }

}

