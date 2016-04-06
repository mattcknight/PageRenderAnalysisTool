using System;
using System.Data.SqlClient;
using ThresholdAnalysis.Utils;

namespace ThresholdAnalysis.Summary
{
    public class Key
    {
        public int[] Values { get; private set; }

        public Key(int numKeys)
        {
            Values = new int[numKeys];
        }

        public override int GetHashCode()
        {
            int hash = 23;

            for (int i = 0; i < Values.Length; i++ )
            {
                hash = hash * 31 + Values[i].GetHashCode();    
            }

            return hash;

        }

        public override bool Equals(Object obj)
        {
            var other = (Key) obj;
            if (other.Values.Length != Values.Length)
                return false;

            for (int i = 0; i < Values.Length; i++)
            {
                if (Values[i] != other.Values[i])
                    return false;
            }

            return true;

        }


        
    }

    public class SummaryRecord
    {
        public DateTime LogDate { get; private set; }
        public Key Key { get; private set; }
        public long[] Measures { get; private set; }
        public byte DataCenter { get; set; }
        public long?[] PrevHourMeasures { get;  set; }
        public long?[] YesterdayMeasures { get;  set; }
        public long?[] Wo1WMeasures { get; set; }
        public long?[] WonWMeasures { get; set; }

        private SummaryRecord()
        {
            //We want the object constructed through static method
        }

 
        public static SummaryRecord Read(SqlDataReader reader, int numKeys, int numMeasures)
        {
            int index = 0;
            var record = new SummaryRecord {LogDate = reader.GetDateTime(index++), Key = new Key(numKeys)};

            for(int i=0; i< numKeys; i++)
            {
                record.Key.Values[i] = SqlReaderUtil.EvalSqlReaderInt(reader, index++, 0);
            }

            record.DataCenter = reader.GetByte(index++);
            record.Measures = new long[numMeasures];
            for (int i = 0; i < numMeasures; i++)
            {
                record.Measures[i] = reader.GetInt64(index++);
            }
            return record;
        }        
    }
}