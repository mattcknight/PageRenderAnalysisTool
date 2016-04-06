using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace ThresholdAnalysis.Utils
{
    public class PageReaderUtil
    {
        public static List<XmlDocument> GetMessageAsXml(string messageDetails, Regex regex)
        {
            var rtnValue = new List<XmlDocument>();
            MatchCollection matchColl = regex.Matches(messageDetails);

            if (matchColl.Count > 0)
            {
                foreach (Match match in matchColl)
                {
                    rtnValue.Add(ConvertToXml(match.Groups[0].Value));
                }
            }
            return rtnValue;
        }

        public static XmlDocument ConvertToXml(string messagePart)
        {
            var doc = new XmlDocument();
            doc.LoadXml(messagePart);
            return doc;
        }

        public static void ExtractSessionInfo(string messageDetails, BasePage page)
        {
            var sessionState = SessionState.Create(messageDetails);

            if (sessionState != null)
            {
                var sid = sessionState.GetSid();

                try
                {
                    page.Sid = !string.IsNullOrEmpty(sid) ? Guid.Parse(sid) : Guid.Empty;
                }
                catch (Exception e)
                {
                    var msg = string.Format("Error Converting Sid:{0} to guid", sid);
                    Logger.LogExceptions.Error(msg, e);
                    throw;
                }
                page.IsSubscriber = sessionState.IsSubscriber();
                page.PageCode = sessionState.GetPageCode();
                page.UserId = sessionState.GetUserId();
            }
            else
            {
                page.Sid = Guid.Empty;
                page.IsSubscriber = false;
                page.UserId = 0;
            }

        }
    }
}
