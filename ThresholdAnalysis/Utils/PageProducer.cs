using System;
using System.Data.SqlClient;

namespace ThresholdAnalysis.Utils
{
    public class PageProducer
    {
        private int _seqNum;
        private readonly int _maxLogSeq;

        public static readonly string PageThresholdMessageText = "Page render threshold%";


        public static readonly string TimeoutMessageText = "Timeout expired%";

        private const string QueryString =
            @"select LogEntryId, LogEntryDateTime, MessageText, WebServer, MessageDetails, TraceData from {0}  (nolock) 
            where LogEntryDateTime > @start and LogEntryId > @logEntryId 
           and MessageDescriptor like @messageDescriptor and MessageText like @messageText and CodeBaseID=36 order by LogEntryId   OPTION (MAXDOP 4)";

        private static readonly string MessageLogQueryString = string.Format(QueryString, "dbo.MessageLog");

        public const int MaxLatencyMinutes = 180;

        private readonly string _messageText;
        private readonly string _messageDescriptor;

        public PageProducer(int seqNum, int maxLogSeq, string messageText, string messageDescriptor = "%")
        {
            _seqNum = seqNum;
            _maxLogSeq = maxLogSeq;
            _messageText = messageText;
            _messageDescriptor = messageDescriptor;
        }

     
        public void Produce(String connectionString, DateTime start,  IPageProcessor consumer)
        {
            Produce(connectionString, MessageLogQueryString, start,  consumer);
        }

        public void Produce(String connectionString,  string query, DateTime start, IPageProcessor consumer)
        {

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                DateTime startFudge = start.AddMinutes(-1 * MaxLatencyMinutes);
                var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@start", startFudge);
                cmd.Parameters.AddWithValue("@logEntryId", _maxLogSeq);
                cmd.Parameters.AddWithValue("@messageText", _messageText);
                cmd.Parameters.AddWithValue("@messageDescriptor", _messageDescriptor);

                cmd.CommandTimeout = 0; //Don't worry about time out!!

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int logEntryId = reader.GetInt32(0);
                        DateTime logEntryDateTime = reader.GetDateTime(1);
                        string pageText = reader.GetString(2);
                        string webserver = reader.GetString(3);
                        string messageDetails = reader.GetString(4);
                        string traceData = reader.GetString(5);

                        var page = new BasePage(_seqNum++, logEntryDateTime, pageText) { WebServer = webserver, LogEntryId = logEntryId, TraceData = traceData };
                        //We are adding this to a queue so that we can speed up execution by using multiple threads to parse XML etc and 
                        //don't block this thread from fetching the results.
                        consumer.Process(page, messageDetails);
                    }
                }
            }
        }
    }
}
