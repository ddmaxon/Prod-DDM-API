using Microsoft.Extensions.ObjectPool;
using SharpCompress.Crypto;
using System.IO;
using Prod_DDM_API.Classes.Db;
using Prod_DDM_API.Data;
using System.Data;
using static System.Net.WebRequestMethods;
using Prod_DDM_API.types;

namespace Prod_DDM_API.Classes
{
    public class CsvLoader
    {
        private List<CsvLine> _csv;
        private MongoController storage;
        private FileInfo _file;
        private string _file_path;

        public CsvLoader(string csvPath = "./")
        {
            this._csv = new List<CsvLine>();
            this.storage = new MongoController();
            this.loadCsv(csvPath);
            this._file_path = csvPath;
            this._file = this.GetFileInfo();
        }

        public void loadCsv(string path)
        {
            var indexOfres = 1;
            using (var reader = new StreamReader($"{path}"))
            {
                while (!reader.EndOfStream)
                {
                    var val = reader.ReadLine();

                    if (val == null)
                        continue;

                    this._csv.Add(new CsvLine(val, indexOfres));
                    indexOfres++;
                }
            }
        }

        public FileInfo GetFileInfo()
        {
            FileInfo oFileInfo = new FileInfo(this._file_path);

            return oFileInfo;
        }

        public DateTime GetCreationTime()
        {
            return this._file.CreationTime;
        }

        public object GetTimeline()
        {
            string initTime = this._csv[0].SplitList()[0];
            string latestTime = this._csv[this._csv.Count - 1].SplitList()[0];

            // Berechne die Differenz zwischen initTime und latestTime
            TimeSpan difference = DateTime.Parse(latestTime) - DateTime.Parse(initTime);

            return new { initTime, latestTime, difference };
        }

        public bool CheckDBData(string timestamp)
        {
            /* var tests = this.storage.Search(new { timestamp });

            if(tests.Count > 0)
            {
                return true;
            }  */

            return false;
        }

        public List<CsvLine> GetCsvLines()
        {
            return _csv;
        }

        public CsvLine GetCsvLine(int lineNr)
        {
            try
            {
                return this._csv[(lineNr - 1)];
            }
            catch (Exception err)
            {
                throw new IndexOutOfRangeException($"Out of range! Max range is {this._csv.Count()}");
            }
        }

        public List<object> SearchSubstringInCsv(string subStr = " ")
        {
            try
            {
                List<object> result = new List<object>();

                foreach (CsvLine str in this._csv)
                {

                    // search subStr in every line
                    if (str.data.ToLower().Contains(subStr.ToLower()))
                    {
                        dynamic item = this.GetIndexOfSearch(str.data);
                        result.Add(new { indexOf = item.indexOf, data = item.subStr });
                    }
                }

                if (result.Count == 0)
                {
                    throw new Exception("422");
                }

                return result;
            }
            catch (Exception err)
            {
                if (err.Message == "422")
                {
                    throw new DataMisalignedException($"No sources found for {subStr}");
                }

                throw new Exception($"Something went wrong! We're working on it.");
            }
        }

        public object GetIndexOfSearch(string subStr)
        {
            List<string> result = new List<string>();
            var indexOfres = 0;

            foreach (CsvLine str in this._csv)
            {
                // search subStr in every line and note the index
                if (str.data.ToLower().Contains(subStr.ToLower()))
                {
                    result.Add(str.data);
                    break;
                }
                indexOfres++;
            }


            return new
            {
                indexOf = indexOfres,
                subStr,
                result
            };
        }

        public object GetDataBetween(string firstSub, string secondSub)
        {
            try
            {
                List<string> _res = new List<string>();

                bool isBetween = false;

                foreach (CsvLine str in this._csv)
                {
                    Console.WriteLine(str);
                    if (str.data.ToLower().Contains(firstSub.ToLower()) && !isBetween)
                    {
                        isBetween = true;
                    }

                    if (isBetween)
                    {
                        _res.Add(str.data);
                    }

                    if (str.data.ToLower().Contains(secondSub.ToLower()) && isBetween)
                    {
                        break;
                    }
                }

                if (_res.Count <= 0)
                {
                    throw new DataException("422");
                }

                return new
                {
                    start = new { firstSub, index = GetIndexOfSearch(firstSub) },
                    endSub = new { secondSub, index = GetIndexOfSearch(secondSub) },
                    isBetween,
                    result = _res,
                    resultCount = _res.Count()
                };
            }
            catch (Exception err)
            {
                if (err.Message == "422")
                {
                    throw new DataException("No srcs found!");
                }

                throw new Exception(err.Message);
            }
        }

        public object GetAllTests()
        {
            string _testdata_startStr = "Start: \"DB_SaveResult\"";
            string _testdata_endStr = "End: \"DB_SaveResult\"";

            List<List<object>> _res = new List<List<object>>();
            List<object> _temp = new List<object>();

            bool isBetween = false;


            //if (!this.CheckDBData(this._csv[0]))
            //{
                foreach (CsvLine str in this._csv)
                {

                    if (str.data.ToLower().Contains(_testdata_startStr.ToLower()) && !isBetween)
                    {
                        isBetween = true;
                        _temp = new List<object>();
                    }

                    if (isBetween)
                    {
                        dynamic item = this.GetIndexOfSearch(str.data);
                        _temp.Add(new { indexOf = item.indexOf, data = item.subStr });
                    }

                    if (str.data.ToLower().Contains(_testdata_endStr.ToLower()) && isBetween)
                    {
                        isBetween = false;
                        _res.Add(_temp);
                    }
                }
            //}

            return new
            {
                data = _res,
                testCount = _res.Count
            };
        }

        public object GetTestSpecificData(List<string> test)
        {
            try
            {
                string[] config = Config.CSVDataConfig;

                // List<object> keyLineList = new List<object>();
                Dictionary<string, string> keyLineList = new Dictionary<string, string>();


                foreach (var line in test)
                {
                    foreach (var key in config)
                    {
                        if (line.Contains(key) && !keyLineList.ContainsKey(line))
                        {
                            keyLineList.Add(line, key);
                        }
                    }
                }


                //foreach (KeyValuePair<string, string> kvp in keyLineList)
                //{

                //}


                return new { };
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }

        public List<double> GetExecutionTime(dynamic res, string selector = null)
        {
            try
            {
                List<double> execTime = new List<double>();

                if (res is List<CsvLine> stringList)
                {
                    List<CsvLine> tempList = stringList;
                    foreach (CsvLine line in tempList)
                    {
                        if(line.GetExecTimeOfLine(selector) != 0)
                        {
                            execTime.Add(line.GetExecTimeOfLine(selector));
                        }
                    }
                }
                else if (res is CsvLine singleString)
                {
                    if (res.GetExecTimeOfLine(selector) != 0)
                    {
                        execTime.Add(res.GetExecTimeOfLine(selector));
                    }
                }

                if (execTime.Count <= 0)
                {
                    throw new Exception($"No data with time found! The selector was {selector}");
                }

                return execTime;
            }
            catch (Exception err)
            {
                throw new FormatException(err.Message);
            }
        }

        public object GetExecutionTimeWithSelectors(double[] avgArr)
        {
            double sum = 0;

            foreach (var line in avgArr)
            {
                sum += line;
            }

            double avg = sum / avgArr.Length;

            double miliseconds = sum * 1000;
            double second = sum;
            double minutes = second / 60;
            double hours = minutes / 60;

            return new { avg, execTime = new { miliseconds, second, minutes, hours }, count = avgArr.Length, values = avgArr };
        }
    }
}
