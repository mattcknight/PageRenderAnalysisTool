using System;
using System.Data.SqlClient;

namespace ThresholdAnalysis.Utils
{
    public class SqlReaderUtil
    {
        public static byte GetByte(SqlDataReader reader, int colIndex, byte nullVal)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetByte(colIndex);
            return nullVal;
        }

        public static bool GetBoolean(SqlDataReader reader, int colIndex, bool nullVal)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetBoolean(colIndex);
            return nullVal;
        }

        public static int GetInt(SqlDataReader reader, int colIndex, int nullVal)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetInt32(colIndex);
            return nullVal;
        }

        public static double GetDecimal(SqlDataReader reader, int colIndex, double nullVal)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetSqlDecimal(colIndex).ToDouble();
            return nullVal;
        }

        public static double GetDouble(SqlDataReader reader, int colIndex, double nullVal)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetDouble(colIndex);
            return nullVal;
        }

        public static short GetShort(SqlDataReader reader, int colIndex, short nullVal)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetInt16(colIndex);
            return nullVal;
        }

        public static long GetLong(SqlDataReader reader, int colIndex, long nullVal)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetInt64(colIndex);
            return nullVal;
        }

        public static DateTime GetDateTime(SqlDataReader reader, int colIndex, DateTime nullVal)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetDateTime(colIndex);
            return nullVal;
        }

        public static string GetString(SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            return null;
        }

        public static string EvalSqlReaderString(SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return Convert.ToString(reader[colIndex]);
            return null;
        }

        public static byte EvalSqlReaderByte(SqlDataReader reader, int colIndex, byte nullValue)
        {
            if (!reader.IsDBNull(colIndex))
                return Convert.ToByte(reader[colIndex]);
            return nullValue;
        }

        public static short EvalSqlReaderShort(SqlDataReader reader, int colIndex, short nullValue)
        {
            if (!reader.IsDBNull(colIndex))
                return Convert.ToInt16(reader[colIndex]);
            return nullValue;
        }

        public static int EvalSqlReaderInt(SqlDataReader reader, int colIndex, int nullValue)
        {
            if (!reader.IsDBNull(colIndex))
                return Convert.ToInt32(reader[colIndex]);
            return nullValue;
        }

        public static double EvalSqlReaderFloat(SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return Convert.ToDouble(reader[colIndex]);
            return 0.0;
        }

        public static int? EvalSqlReaderNullInt32(SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return Convert.ToInt32(reader[colIndex]);
            return null;
        }


        public static bool EvalSqlReaderBool(SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return Convert.ToBoolean(reader[colIndex]);
            return false;
        }
    }
}
