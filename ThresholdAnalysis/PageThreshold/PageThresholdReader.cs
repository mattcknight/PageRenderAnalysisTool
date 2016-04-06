using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using ThresholdAnalysis.utils;
using ThresholdAnalysis.Utils;

namespace ThresholdAnalysis.PageThreshold
{
    class PageThresholdReader : ITask
    {
        private const string NullDateTime = "01/01/2010";
        private readonly PageThresholdPage _pageThresholdPage;
        private readonly String _messageDetails;
        private readonly PersistThreshold _persistThreshold;

        private readonly Regex _detailsRegEx = new Regex(@"<details>.*</details>",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private readonly Regex _dbCallsRegEx = new Regex(@"<DbTrace>.*</DbTrace>",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private readonly Regex _mrsixCallsRegEx = new Regex(@"<MrSixTrace>.*</MrSixTrace>",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);


        private readonly Regex _apiCallsRegEx = new Regex(@"<ApiTrace>.*</ApiTrace>",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        private const string RenderTime = "Render Time";


        public PageThresholdReader(PageThresholdPage pageThresholdPage, string messageDetails, PersistThreshold persistThreshold)
        {
            _pageThresholdPage = pageThresholdPage;
            _messageDetails = messageDetails;
            _persistThreshold = persistThreshold;
        }





        private static TimeSpan ExtractRenderTimeSpan(string renderString, PageThresholdPage page)
        {
            try
            {
                int index = renderString.IndexOf(RenderTime, StringComparison.Ordinal);
                if (index > -1)
                {
                    string renderTime = renderString.Substring(index + RenderTime.Length);

                    int renderIndex = renderTime.IndexOf("\r\n", StringComparison.Ordinal);

                    if (renderIndex > 0)
                    {
                        int len = renderIndex - 1;
                        renderTime = renderTime.Substring(0, len);
                    }
                    
                    return TimeSpan.Parse(renderTime);
                }
                return TimeSpan.MinValue;
            }
            catch (Exception)
            {   
                Logger.LogExceptions.InfoFormat("Unable to parse time span for {0} renderString, LogId = {1}, LogEntryDateTime = {2}", renderString, page.LogEntryId,
                    page.LogEntryDate);
                
                throw;
            }
 
        }

        private static XmlElement GetFirstChildElement(XmlElement elem, string tagName)
        {
            XmlNodeList list = elem.GetElementsByTagName(tagName);
            if (list.Count > 0)
                return (XmlElement)list[0];
            return null;
        }

        private static string GetPropertyValue(XmlElement elem, string tagName)
        {
            XmlElement childElem = GetFirstChildElement(elem, tagName);
            if (childElem != null)
                return childElem.InnerText;
            return null;
        }

        private static int ExtractUserId(XmlElement dataCall)
        {
            var elem = GetFirstChildElement(dataCall, "Parameters");

            if (elem == null)
                return 0;

            XmlNodeList list = elem.GetElementsByTagName("Parameter");

            foreach (var node in list)
            {
                string name = GetPropertyValue((XmlElement)node, "Name");
                if (name.Equals("@UserID"))
                {
                    string uidString = GetPropertyValue((XmlElement)node, "Value");
                    if (!string.IsNullOrEmpty(uidString))
                        return Convert.ToInt32(uidString);
                }
            }

            return 0;
        }

        private static void AssignBeginAndEndTime(XmlElement dataCall, DbCall dbcall)
        {
            string beginTime = GetPropertyValue(dataCall, "BeginTime");
            if (string.IsNullOrEmpty(beginTime))
                beginTime = NullDateTime;
            dbcall.Begin = DateTime.Parse(beginTime);
            string endTime = GetPropertyValue(dataCall, "EndTime");
            if (string.IsNullOrEmpty(endTime))
                endTime = NullDateTime;
            dbcall.End = DateTime.Parse(endTime);
            if (beginTime != NullDateTime && endTime != NullDateTime)
            {
                var ts = (dbcall.End - dbcall.Begin);
                dbcall.ComputedExecutionTime = Convert.ToInt64(ts.TotalMilliseconds);
            }
            else
            {
                dbcall.ComputedExecutionTime = dbcall.ExecutionTime;
            }

        }

        private static TimeSpan ExtractTimeSpan(XmlElement dataCall, string propName)
        {
            string execution = GetPropertyValue(dataCall, propName);
            TimeSpan timeSpan = TimeSpan.MinValue;
            if (execution != null)
                timeSpan = TimeSpan.Parse(execution);
            return timeSpan;
        }


        private static DbCall ExtractDbItem(XmlElement dataCall, int procSeq)
        {
            string procName = GetPropertyValue(dataCall, "Command");
            string datasourceAlias = GetPropertyValue(dataCall, "DataSourceAlias");
            var timeSpan = ExtractTimeSpan(dataCall, "TotalTime");
            var dbcall = new DbCall(procName, datasourceAlias, Convert.ToInt64(timeSpan.TotalMilliseconds))
            {
                ProcSeq = procSeq
            };

            AssignBeginAndEndTime(dataCall, dbcall);

            return dbcall;

        }

        private static DbCall ExtractMrSixItem(XmlElement dataCall, int procSeq)
        {
            string procName = GetPropertyValue(dataCall, "SearchType");
            const string datasourceAlias = "MrSix";
            TimeSpan timeSpan = ExtractTimeSpan(dataCall, "ExecutionTime");
            var dbcall = new DbCall(procName, datasourceAlias, Convert.ToInt64(timeSpan.TotalMilliseconds))
            {
                ProcSeq = procSeq
            };
            AssignBeginAndEndTime(dataCall, dbcall); dbcall.ProcSeq = procSeq;
            return dbcall;
        }

        private static DbCall ExtractApiItem(XmlElement dataCall, int procSeq)
        {
            string procName = GetPropertyValue(dataCall, "ApiName");
            string datasource = GetPropertyValue(dataCall, "DataSource");
            TimeSpan timeSpan = ExtractTimeSpan(dataCall, "ExecutionTime");

            var elem = GetFirstChildElement(dataCall, "Parameters");

            var dbcall = new DbCall(procName, datasource, Convert.ToInt64(timeSpan.TotalMilliseconds))
            {
                ProcSeq = procSeq
            };
            AssignBeginAndEndTime(dataCall, dbcall);
            dbcall.ProcSeq = procSeq;

            if (elem != null )
            {
                XmlNodeList list = elem.GetElementsByTagName("Parameter");

                foreach (var node in list)
                {
                    string name = GetPropertyValue((XmlElement)node, "Name");
                    if ( !string.IsNullOrEmpty(name)  && name.Equals("ServerDurationInMillis"))
                    {
                        string serverDuration = GetPropertyValue((XmlElement)node, "Value");
                        if (!string.IsNullOrEmpty(serverDuration))
                            dbcall.ServerDuration = Convert.ToInt64(serverDuration);
                    }
                    else if ( !string.IsNullOrEmpty(name)  && ( name.Equals("ServerName") || name.Equals("MrSIXServerName") ) )
                    {
                        dbcall.ServerName = GetPropertyValue((XmlElement)node, "Value");
                    }
                }
                
            }
            return dbcall;
        }


        public void Execute()
        {
            try
            {
                if (ReadMessageDetails(_pageThresholdPage, _messageDetails))
                {
                    _persistThreshold.Add(_pageThresholdPage);
                }
            }
            catch (Exception e)
            {
                Logger.LogExceptions.Error("Error Reading PageThresholdPage", e);
                throw;
            }
        }


        public bool ReadMessageDetails(PageThresholdPage pageThresholdPage, string messageDetails)
        {
            List<XmlDocument> detailsMsgDoc = PageReaderUtil.GetMessageAsXml(messageDetails, _detailsRegEx);

            if (detailsMsgDoc.Count == 0)
                return false;

            XmlElement detailsMsg = detailsMsgDoc[0].DocumentElement;
            if (detailsMsg == null)
                return false;

            PageReaderUtil.ExtractSessionInfo(messageDetails, pageThresholdPage);

            
            TimeSpan ts = ExtractRenderTimeSpan(detailsMsg.InnerText, pageThresholdPage);

            pageThresholdPage.ExecutionTime = Convert.ToInt64(ts.TotalMilliseconds);

            List<XmlDocument> dbCallsDoc = PageReaderUtil.GetMessageAsXml(messageDetails, _dbCallsRegEx);
            int procSeq = 0;

            if (dbCallsDoc.Count != 0)
            {
                XmlElement dbCalls = dbCallsDoc[0].DocumentElement;

                if (dbCalls != null)
                {
                    foreach (XmlElement dataCallElement in dbCalls.GetElementsByTagName("DataCall"))
                    {
                        DbCall dbCall = ExtractDbItem(dataCallElement, procSeq++);

                        pageThresholdPage.AddDbItem(dbCall);

                        if (!pageThresholdPage.HasLoginAuth)
                        {
                            pageThresholdPage.HasLoginAuth = dbCall.IsLoginAuth();
                        }

                        if (pageThresholdPage.UserId == 0 && dbCall.IdentifiesUserId())
                            pageThresholdPage.UserId = ExtractUserId(dataCallElement);

                    }
                }

            }


            List<XmlDocument> mrSixCallsDoc = PageReaderUtil.GetMessageAsXml(messageDetails, _mrsixCallsRegEx);
            if (mrSixCallsDoc.Count != 0)
            {
                XmlElement mrSixCalls = mrSixCallsDoc[0].DocumentElement;

                if (mrSixCalls != null)
                {
                    foreach (XmlElement dataCallElement in mrSixCalls.GetElementsByTagName("DataCall"))
                    {

                        DbCall dbCall = ExtractMrSixItem(dataCallElement, procSeq++);
                        pageThresholdPage.AddDbItem(dbCall);
                    }
                }

            }

            List<XmlDocument> apiCallsDoc = PageReaderUtil.GetMessageAsXml(messageDetails, _apiCallsRegEx);
            if (apiCallsDoc.Count != 0)
            {
                var apiCalls = apiCallsDoc[0].DocumentElement;

                if (apiCalls != null)
                {
                    foreach (XmlElement dataCallElement in apiCalls.GetElementsByTagName("DataCall"))
                    {

                        DbCall dbCall = ExtractApiItem(dataCallElement, procSeq++);
                        pageThresholdPage.AddDbItem(dbCall);
                    }                    
                }
            }

            return true;
        }

  


    }
}
