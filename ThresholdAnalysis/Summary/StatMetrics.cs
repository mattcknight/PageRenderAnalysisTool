using System;
using System.Collections.Generic;

namespace ThresholdAnalysis.Summary
{
    public class StatMetrics
    {
        public static void Compute(List<SummaryRecord> currRecords, List<SummaryRecord> histRecords, int nWeek)
        {
            //Add records in a dictionary for quick look up
            var dictionary = new Dictionary<DateTime, Dictionary<Key, SummaryRecord>>();
            Add(dictionary, currRecords);
            Add(dictionary, histRecords);

            FillPrevHourMetrics(currRecords, dictionary);
            FillYesterdayMetrics(currRecords, dictionary);
            FillNWeekMetrics(currRecords, dictionary, nWeek);
            FillPrevWeekMetrics(currRecords, dictionary);
        }

        private static void Add(Dictionary<DateTime, Dictionary<Key, SummaryRecord>> dictionary, IEnumerable<SummaryRecord> records)
        {
            foreach (var record in records)
            {
                Dictionary<Key, SummaryRecord> dict2;
                if (!dictionary.TryGetValue(record.LogDate, out dict2))
                {
                    dict2 = new Dictionary<Key, SummaryRecord>();
                    dictionary[record.LogDate] = dict2;
                }
                dict2[record.Key] = record;
            }
        }

        private static SummaryRecord Lookup(Dictionary<DateTime, Dictionary<Key, SummaryRecord>> dictionary, DateTime dateTime, Key key)
        {
            Dictionary<Key, SummaryRecord> dict2;
            if (dictionary.TryGetValue(dateTime, out dict2))
            {
                SummaryRecord record;
                if (dict2.TryGetValue(key, out record))
                    return record;
            }
            return null;
        }

        private static void FillPrevHourMetrics(IEnumerable<SummaryRecord> records, Dictionary<DateTime, Dictionary<Key, SummaryRecord>> dictionary)
        {
            foreach (var record in records)
            {
                record.PrevHourMeasures = new long?[record.Measures.Length];

                var key = new Key(2);
                key.Values[0] = record.Key.Values[0] == 0 ? 23 : record.Key.Values[0] - 1;
                key.Values[1] = record.Key.Values[1];

                var prevHourRecord = Lookup(dictionary, record.Key.Values[0] == 0 ? record.LogDate.AddDays(-1) : record.LogDate, key);

                for (int i = 0; i < record.Measures.Length; i++)
                {
                    if (prevHourRecord != null)
                        record.PrevHourMeasures[i] = prevHourRecord.Measures[i];
                    else
                        record.PrevHourMeasures[i] = null;
                }
            }
        }

        private static void FillYesterdayMetrics(IEnumerable<SummaryRecord> records, Dictionary<DateTime, Dictionary<Key, SummaryRecord>> dictionary)
        {
            foreach (var record in records)
            {
                record.YesterdayMeasures = new long?[record.Measures.Length];
                var yesterdayRecord = Lookup(dictionary, record.LogDate.AddDays(-1), record.Key);
                for (int i = 0; i < record.Measures.Length; i++)
                {
                    if (yesterdayRecord != null)
                        record.YesterdayMeasures[i] = yesterdayRecord.Measures[i];
                    else
                        record.YesterdayMeasures[i] = null;
                }
            }
        }


        private static List<SummaryRecord> GetNWeekDates(Dictionary<DateTime, Dictionary<Key, SummaryRecord>> dictionary, DateTime logDate, Key key, int nWeeks)
        {
            var rtnValue = new List<SummaryRecord>();
            for (int i = 0; i < nWeeks; i++)
            {
                var prevWeek = logDate.AddDays(-1 * (i + 1) * 7);
                var record = Lookup(dictionary, prevWeek, key);
                if (record != null)
                    rtnValue.Add(record);

            }
            return rtnValue;
        }

        private static long? GetAverageMeasure(List<SummaryRecord> prevWeeks, int index)
        {
            long total = 0;
            foreach (var record in prevWeeks)
            {
                total += record.Measures[index];
            }
            return (total / prevWeeks.Count);
        }

        private static void AssignNWeekAverages(SummaryRecord record, List<SummaryRecord> prevWeeks)
        {
            int numMeasures = record.Measures.Length;
            record.WonWMeasures = new long?[numMeasures];
            if (prevWeeks.Count == 0)
            {
                for (int i = 0; i < numMeasures; i++)
                {
                    record.WonWMeasures[i] = null;
                }
            }
            else
            {
                for (int i = 0; i < numMeasures; i++)
                {
                    record.WonWMeasures[i] = GetAverageMeasure(prevWeeks, i);
                }
            }
        }



        private static void FillNWeekMetrics(IEnumerable<SummaryRecord> records, Dictionary<DateTime, Dictionary<Key, SummaryRecord>> dictionary, int nWeeks)
        {
            foreach (var record in records)
            {
                AssignNWeekAverages(record, GetNWeekDates(dictionary, record.LogDate, record.Key, nWeeks));
            }

        }

        private static void FillPrevWeekMetrics(IEnumerable<SummaryRecord> records, Dictionary<DateTime, Dictionary<Key, SummaryRecord>> dictionary)
        {
            foreach (var record in records)
            {
                int numMeasures = record.Measures.Length;
                record.Wo1WMeasures = new long?[numMeasures];
                var lastWeekRecord = Lookup(dictionary, record.LogDate.AddDays(-7), record.Key);
                for (int i = 0; i < numMeasures; i++)
                {
                    if (lastWeekRecord != null)
                    {
                        record.Wo1WMeasures[i] = lastWeekRecord.Measures[i];                        
                    }
                    else
                    {
                        record.Wo1WMeasures[i] = null;
                    }
                }
            }
        }

 
    }
}
