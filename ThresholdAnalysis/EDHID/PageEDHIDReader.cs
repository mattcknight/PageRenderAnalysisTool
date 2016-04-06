using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using ThresholdAnalysis.Utils;
using ThresholdAnalysis.utils;

namespace ThresholdAnalysis.EDHID
{
    public class PageEdhidReader : ITask
    {
        private readonly Regex _detailsRegEx = new Regex(@"<details>.*</details>",
                                                         RegexOptions.Compiled | RegexOptions.IgnoreCase |
                                                         RegexOptions.Singleline);

        private const string TierPrefix = "Exception Message: ";
        private const string TierPostfix = "<br>Exception Source";

        private readonly PageEdhidPage _edhidPage;
        private readonly String _messageDetails;
        private readonly PersistEdhid _persistEdhid;

        public PageEdhidReader(PageEdhidPage edhidPage, string messageDetails, PersistEdhid persistEdhid)
        {
            _edhidPage = edhidPage;
            _messageDetails = messageDetails;
            _persistEdhid = persistEdhid;
        }

        private string ExtractMessageText(string detailsText)
        {
            if (string.IsNullOrEmpty(detailsText))
                return null;

            int index = detailsText.IndexOf(TierPrefix, StringComparison.Ordinal);

            if (index < 0)
                return null;

            int newPos = index + TierPrefix.Length;

            if (detailsText.Length <= newPos)
                return null;

            var newText = detailsText.Substring(newPos);

            int index2 = newText.IndexOf(TierPostfix, StringComparison.Ordinal);

            if (index2 <= 0)
                return null;

            return newText.Substring(0, index2);
        }

        protected bool ReadMessageDetails(PageEdhidPage page, string messageDetails)
        {
            List<XmlDocument> detailsMsgDoc = PageReaderUtil.GetMessageAsXml(messageDetails, _detailsRegEx);

            if (detailsMsgDoc.Count == 0)
                return false;

            XmlElement detailsMsg = detailsMsgDoc[0].DocumentElement;

            if (detailsMsg == null)
                return false;

            page.Details = detailsMsg.InnerText;

            if (string.IsNullOrEmpty(page.Details))
                page.Details = "Null";

            page.MessageText = ExtractMessageText(detailsMsg.InnerText) ?? "Unknown";

            if (string.IsNullOrEmpty(page.TraceData))
                page.TraceData = "Null";

            PageReaderUtil.ExtractSessionInfo(messageDetails, page);

            return true;
        }

        public void Execute()
        {
            try
            {
                if ( ReadMessageDetails(_edhidPage, _messageDetails) )
                    _persistEdhid.Add(_edhidPage);
            }
            catch (Exception ex)
            {
                Logger.LogExceptions.Error("Error Reading EDHID Page", ex);
                throw;
            }
        }


    }
}