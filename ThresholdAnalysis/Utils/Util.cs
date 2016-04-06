using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using ThresholdAnalysis.utils;

namespace ThresholdAnalysis.Utils
{
    public class Util
    {
        private static readonly DateTime NullDate = new DateTime(1970, 1, 1);

        public static string ExtractPageText(string prefixToStrip, string messageText )
        {
            return messageText.Substring(prefixToStrip.Length - 1);
        }

        public static int GetMaxProcessedLogSequence(string writeConnection, string query)
        {
            int logSequence = 0;
            using (var conn = new SqlConnection(writeConnection))
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        logSequence = SqlReaderUtil.GetInt(reader, 0, int.MinValue);
                    }
                }
            }
            return logSequence;
        }



        public static DateTime GetMaxProcessedDate(string writeConnection, string query, out int nextSeqNum)
        {
            nextSeqNum = 0;
            var rtnValue = NullDate;
            using (var conn = new SqlConnection(writeConnection))
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        rtnValue = SqlReaderUtil.GetDateTime(reader, 0, NullDate);
                        nextSeqNum = SqlReaderUtil.GetInt(reader, 1, -1);
                        nextSeqNum++;

                    }
                }
            }
            return rtnValue;
        }

        public static void PrintQueueLength(TaskProcessor processor, int sleepTime)
        {
            //This is a diagnostic to understand where the bottle necks are.
            while (true)
            {
                if (processor != null)
                {
                    Console.WriteLine("Queue Length =" + processor.GetPendingCount());
                }
                Thread.Sleep(sleepTime);
            }
// ReSharper disable once FunctionNeverReturns
        }

        public static void PopulateLookup(SqlConnection connection, IDictionary<string, int> dictionary, string table)
        {
            using (var cmd = new SqlCommand("select * from " + table, connection))
            {
                cmd.CommandType = CommandType.Text;
                using (var reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        string currString = reader.GetString(1);

                        if(!dictionary.ContainsKey(currString))
                            dictionary.Add(currString, reader.GetInt32(0));
                    }
                }
            }
        }

        public static int GetId(IDictionary<string, int> map, string text, DataTable dataTable)
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

    }
}
