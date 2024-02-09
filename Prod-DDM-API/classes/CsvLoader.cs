using Microsoft.Extensions.ObjectPool;
using SharpCompress.Crypto;
using System.IO;
using Prod_DDM_API.Classes.Db;
using Prod_DDM_API.Data;
using System.Data;
using static System.Net.WebRequestMethods;

namespace Prod_DDM_API.Classes
{
    public class CsvLoader
    {
        private List<string> _csv;
        private MongoController storage;
        private FileInfo _file;
        private string _file_path;

        public CsvLoader(string csvPath = "./")
        {
            this._csv = new List<string>();
            this.storage = new MongoController();
            this.loadCsv(csvPath);
            this._file_path = csvPath;
            this._file = this.GetFileInfo();
        }

        public void loadCsv(string path)
        {
            using (var reader = new StreamReader($"{path}"))
            {
                while (!reader.EndOfStream)
                {
                    var val = reader.ReadLine();

                    if (val == null)
                        continue;

                    this._csv.Add(val);
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

        public bool CheckDBData(string timestamp)
        {
            /* var tests = this.storage.Search(new { timestamp });

            if(tests.Count > 0)
            {
                return true;
            }  */

            return false;
        }

        public List<string> GetCsvLines()
        {
            return _csv;
        }

        public string GetCsvLine(int lineNr)
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

                foreach (string str in this._csv)
                {

                    // search subStr in every line
                    if (str.ToLower().Contains(subStr.ToLower()))
                    {
                        dynamic item = this.GetIndexOfSearch(str);
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

            foreach (string str in this._csv)
            {
                // search subStr in every line and note the index
                if (str.ToLower().Contains(subStr.ToLower()))
                {
                    result.Add(str);
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

                foreach (string str in this._csv)
                {
                    Console.WriteLine(str);
                    if (str.ToLower().Contains(firstSub.ToLower()) && !isBetween)
                    {
                        isBetween = true;
                    }

                    if (isBetween)
                    {
                        _res.Add(str);
                    }

                    if (str.ToLower().Contains(secondSub.ToLower()) && isBetween)
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
                foreach (string str in this._csv)
                {

                    if (str.ToLower().Contains(_testdata_startStr.ToLower()) && !isBetween)
                    {
                        isBetween = true;
                        _temp = new List<object>();
                    }

                    if (isBetween)
                    {
                        dynamic item = this.GetIndexOfSearch(str);
                        _temp.Add(new { indexOf = item.indexOf, data = item.subStr });
                    }

                    if (str.ToLower().Contains(_testdata_endStr.ToLower()) && isBetween)
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

    }
}
