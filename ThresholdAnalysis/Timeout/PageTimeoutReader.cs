using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using ThresholdAnalysis.Utils;
using ThresholdAnalysis.utils;

namespace ThresholdAnalysis.Timeout
{
    public class PageTimeoutReader : ITask
    {
        private readonly Regex _detailsRegEx = new Regex(@"<details>.*</details>",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private const string TierPrefix =
            "The timeout period elapsed prior to completion of the operation or the server is not responding.<br><b>Procedure = </b><br><b>Server = </b>";

        private const string TierPostFix = "<br><b>State";

        private readonly TimeoutPage _timeoutPage;
        private readonly String _messageDetails;
        private readonly PersistTimeout _persistTimeout;

        public PageTimeoutReader(TimeoutPage timeoutPage, string messageDetails, PersistTimeout persistTimeout)
        {
            _timeoutPage = timeoutPage;
            _messageDetails = messageDetails;
            _persistTimeout = persistTimeout;
        }


        private string ExtractDataSourceName(string detailsText)
        {
            if (string.IsNullOrEmpty(detailsText))
                return null;


            int index = detailsText.IndexOf(TierPrefix, StringComparison.Ordinal);
            if (index < 0)
                return null;

            int newPos = index + TierPrefix.Length;

            if (detailsText.Length <= newPos)
                return null;

            var newText =  detailsText.Substring(newPos);

            int index2 = newText.IndexOf(TierPostFix, StringComparison.Ordinal);

            if (index2 <= 0)
                return null;

            return newText.Substring(0, index2);

       }

        protected bool ReadMessageDetails(TimeoutPage page, string messageDetails)
        {
            List<XmlDocument> detailsMsgDoc = PageReaderUtil.GetMessageAsXml(messageDetails, _detailsRegEx);

            if (detailsMsgDoc.Count == 0)
                return false;

            XmlElement detailsMsg = detailsMsgDoc[0].DocumentElement;
            if (detailsMsg == null)
                return false;

            page.Details = detailsMsg.InnerText;
            if (string.IsNullOrEmpty(page.Details) )
            {
                page.Details = "Null";
            }
            
            page.DataSourceName = ExtractDataSourceName(detailsMsg.InnerText) ?? "Unknown";


            if (string.IsNullOrEmpty(page.TraceData))
            {
                page.TraceData = "Null";
            }

            PageReaderUtil.ExtractSessionInfo(messageDetails, page);

            return true;


        }

        public void Execute()
        {
            try
            {
                if (ReadMessageDetails(_timeoutPage, _messageDetails))
                {
                    _persistTimeout.Add(_timeoutPage);
                }
            }
            catch (Exception e)
            {
                Logger.LogExceptions.Error("Error Reading PageThresholdPage", e);
                throw;
            }
        }


        public static void Test(string fileName)
        {
            string messageDetails;
            using (var sr = new StreamReader(fileName))
            {
                messageDetails = sr.ReadToEnd();
            }

            var timeoutPage = new TimeoutPage(0, DateTime.Now, "dummy");

            var reader = new PageTimeoutReader(timeoutPage, messageDetails, new PersistTimeout(null) );
            reader.ReadMessageDetails(timeoutPage, messageDetails);

            Console.WriteLine("Data Source = {0}", timeoutPage.DataSourceName);


        }

    }
}
