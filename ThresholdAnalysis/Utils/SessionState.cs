using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace ThresholdAnalysis.Utils
{
    public class SessionState
    {
        private static readonly Regex SessionStateRegEx = new Regex(@"<sessionstate>.*</sessionstate>",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private const string IsSubscriberToken = "\"IsSubscriber\":";
        private const string PageCodeToken = "\"PageCode\":";
        private const string SessionSearchEnd = ",";
        private const string SidToken = "\"Sid\":";
        private const string UserIdToken = "\"UserId\":";

        private readonly string _sessionState;

        
        private SessionState(string sessionState)
        {
            _sessionState = sessionState;
        }

        public static SessionState Create(string messageDetails)
        {
            //Get the session details
            List<XmlDocument> dbSessionDoc = PageReaderUtil.GetMessageAsXml(messageDetails, SessionStateRegEx);
            if (dbSessionDoc.Count == 0)
                return null;

            XmlElement dbSession = dbSessionDoc[0].DocumentElement;

            return (dbSession == null) ? null : new SessionState(dbSession.InnerText);
        }

        public String GetSid()
        {
            return ToString(GetSessionProperty(_sessionState, SidToken));
        }

        public bool IsSubscriber()
        {
            return ToBool(GetSessionProperty(_sessionState, IsSubscriberToken));
        }

        public int? GetPageCode()
        {
            return ToInt(GetSessionProperty(_sessionState, PageCodeToken));
        }

        public int GetUserId()
        {
            int? uid = ToInt(GetSessionProperty(_sessionState, UserIdToken));
            if (uid == null)
                return 0;

            return (int) uid;
        }

        private static bool ToBool(string propVal)
        {
            return "true".Equals(propVal);
        }

        private static string ToString(string propVal)
        {
            if (propVal.Length - 2 > 0)
                return propVal.Substring(1, propVal.Length - 2);
            return null;
        }

        public static int? ToInt(string propVal)
        {
            if (string.IsNullOrEmpty(propVal))
                return null;

            int rtnVal;
            if (Int32.TryParse( propVal, out rtnVal))
                return rtnVal;

            return null;
        }

        private static string GetSessionProperty(string sessionState, string propName)
        {
            try
            {
                string rtnVal = string.Empty;

                int index = sessionState.IndexOf(propName, StringComparison.Ordinal);
                int index1 = -1;

                if (index > 0)
                    index1 = sessionState.IndexOf(SessionSearchEnd, index, StringComparison.Ordinal);

                if (index > 0 && index1 > 0)
                {
                    rtnVal = sessionState.Substring(index + propName.Length, index1 - (index + propName.Length));
                }
                return rtnVal;
            }
            catch (Exception e)
            {
                var msg = string.Format("Error Accessing Session State : Session State = {0}, Property={1}", sessionState, propName);
                Logger.LogExceptions.Error(msg, e); 
                throw;
            }
        }

    }
}
