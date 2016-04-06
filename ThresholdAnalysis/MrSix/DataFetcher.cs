using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ThresholdAnalysis.MrSix
{
    public class SearchSummary
    {
        public string SrarchName { get; set; }
        public int Total { get; set; }
        public int TotalLoggedIn { get; set; }
        public int TotalNonLoggedIn { get; set; }
        public int TotalZero { get; set; }
        public int TotalZeroLoggedIn { get; set; }
        public int TotalZeroNonLoggedIn { get; set; }
    }


    public class DataFetcher
    {
        private readonly string _readConnection;

        private readonly string[] _sql = {
                                   "SELECT  Searchname, Count(*) Total FROM [ProcLog].[dbo].[ProcLogMrSIX] Where LogDay = @logDay Group by Searchname",
                                   "SELECT  Searchname,  Count(*) TotalLoggedIn  FROM [ProcLog].[dbo].[ProcLogMrSIX]  Where LogDay = @logDay  and searcheruserId > 0  Group by Searchname",
                                   "SELECT  Searchname, Count(*) TotalNonLoggedIn FROM [ProcLog].[dbo].[ProcLogMrSIX] Where LogDay = @logDay and searcheruserId = 0 Group by Searchname",
                                   "SELECT  SearchName, COUNT(*) TotalZero FROM [ProcLog].[dbo].[ProcLogMrSIX] Where LogDay = @logDay and Rows = 0 Group by Searchname",
                                   "SELECT  Searchname, Count(*) TotalZeroLoggedIn FROM [ProcLog].[dbo].[ProcLogMrSIX] Where LogDay = @logDay and Rows = 0  and searcheruserId > 0 Group by Searchname",
                                   "SELECT  Searchname,   Count(*) TotalZeroNonLoggedIn   FROM [ProcLog].[dbo].[ProcLogMrSIX]   Where LogDay = @logDay and Rows = 0  and searcheruserId = 0  Group by Searchname"};

        public DataFetcher(string readConnection)
        {
            _readConnection = readConnection;
        }


        public IEnumerable<SearchSummary> GetResults(DateTime date)
        {
            IDictionary<string, SearchSummary> result = new Dictionary<string, SearchSummary>();

            using (var connection = new SqlConnection(_readConnection))
            {
                connection.Open();
                for (int i = 0; i < _sql.Count(); i++)
                {
                    using (var cmd = new SqlCommand(_sql[i], connection))
                    {
                        cmd.Parameters.Add(new SqlParameter("@logDay", date.Date));
                        cmd.CommandTimeout = 10*60*1000;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var searchName = reader.GetString(0);
                                var count = reader.GetInt32(1);

                                if (!result.ContainsKey(searchName))
                                {
                                    result.Add(searchName, new SearchSummary {SrarchName = searchName});
                                }

                                var summary = result[searchName];

                                switch (i)
                                {
                                    case 0:
                                        summary.Total = count;
                                        break;
                                    case 1:
                                        summary.TotalLoggedIn = count;
                                        break;
                                    case 2:
                                        summary.TotalNonLoggedIn = count;
                                        break;
                                    case 3:
                                        summary.TotalZero = count;
                                        break;
                                    case 4:
                                        summary.TotalZeroLoggedIn = count;
                                        break;
                                    case 5:
                                        summary.TotalZeroNonLoggedIn = count;
                                        break;
                                }

                            }
                        }
                    }
                }


                return result.Values;
            }
        }
    }

}