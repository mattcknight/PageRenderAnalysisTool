using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ThresholdAnalysis
{
    public class TraceFile
    {
        private static StreamWriter _dataTraceFile; 

        public TraceFile(string fileName)
        {
            _dataTraceFile = new StreamWriter(fileName);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void WriteLineTrace(string line)
        {
            if (_dataTraceFile != null)
            {
                _dataTraceFile.WriteLine(line);
                _dataTraceFile.Flush();
            }
        }

        public void WriteExceptionTrace(Exception e)
        {
            string exceptionDetails = string.Format(@"
<ExceptionDetails>
    <Time>{0}</Time>
    <Message><![CDATA[{1}]]> </Message>
    <StackTrace><![CDATA[{2}]]></StackTrace>
</ExceptionDetails>", DateTime.Now, e.Message, e.StackTrace);  
            WriteLineTrace(exceptionDetails);
        }
    }
}


