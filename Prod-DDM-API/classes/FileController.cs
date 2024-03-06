using Prod_DDM_API.Classes.Db;
using Prod_DDM_API.Data;
using Prod_DDM_API.Types;
using Prod_DDM_API.Types.History;
using System.Data;
using System.IO;

namespace Prod_DDM_API.Classes
{
    public class FileController
    {
        public bool _isDataLocked = false;


        private string name = "";
        private string _file_path;


        private Storage _storage; 
        private FileInfo _file; 
        private List<CsvLine> _csv;
        private DateTime _creation_time;

         
        public FileController(string csvPath = "./")
        {
            this._csv = new List<CsvLine>();
            this._storage = new Storage();

            this._file_path = csvPath;

            this.loadCsv();
            this._file = this.GetFileInfo();
            this._creation_time = this.GetCreationTime();
        }

        public void setFilePath(string newpath)
        {
            this._file_path = newpath;
        }
        public void loadCsv()
        {
            var indexOfres = 1;
            using (var reader = new StreamReader($"{this._file_path}"))
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
        public FCOutput transportToOutput(string name = null)
        {
            if (name == null)
            {
                name = this.name;
            }

            string outputPath = "./output/" + name;
            string inputPath = "./csv/" + name;

            try
            {
                if (!File.Exists(inputPath))
                {
                    // This statement ensures that the file is created,
                    // but the handle is not kept.
                    using (FileStream fs = File.Create(inputPath)) { }
                }

                // Ensure that the target does not exist.
                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                // Move the file.
                File.Move(inputPath, outputPath);

                this._file_path = outputPath;

                // See if the original exists now.
                if (File.Exists(inputPath))
                {
                    return GetFormatedOutput(outputPath, inputPath, "FileTransportToOutput", "The original file still exists, which is unexpected.");
                }
                else
                {
                    return GetFormatedOutput(outputPath, inputPath, "FileTransportToOutput");
                }
            }
            catch (Exception e)
            {
                return GetFormatedOutput(outputPath, inputPath, "FileTransportToOutput", $"The process failed: {e.Message}");
            }
        }
        public FCOutput GetFormatedOutput(string newPath, string oldPath, string method, string message = "Method was successfully executed!")
        {
            FCOutput fCOutput = new FCOutput();

            fCOutput.newPath = newPath;
            fCOutput.oldPath = oldPath;
            fCOutput.method = method;
            fCOutput.message = message;

            return fCOutput;
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
        public List<CsvLine> SearchSubstringInCsv(string subStr = " ")
        {
            try
            {
                List<CsvLine> result = new List<CsvLine>();

                foreach (CsvLine str in this._csv)
                {

                    // search subStr in every line
                    if (str.data.ToLower().Contains(subStr.ToLower()))
                    {
                        dynamic item = this.GetIndexOfSearch(str.data);

                        result.Add(str);
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
        public List<CsvLine> GetFilteredTests(List<CsvLine> csvTests)
        {
            List<CsvLine> csvLines = new List<CsvLine>();

            int testsCount = csvTests.Count / 6; // Wieso 6? in csvTests wurden pro test 6 lines eingenommen (von Levin abgezählt (von Hand :C (Mir geht es so schlecht ;C)))

            csvTests.Reverse();

            for (int i = 0; i < testsCount * 2; i += 2)
            {
                csvLines.Add(csvTests[i]);
            }

            csvLines.Reverse();

            return csvLines;
        }
        public List<CsvLine> GetFilteredTests2(List<List<CsvLine>> csvTests)
        {
            List<CsvLine> csvLines = new List<CsvLine>();

            foreach (var line in csvTests)
            {
                line.ToArray();
                csvLines.Add(line[line.Count() - 2]);
            }

            return csvLines;
        }
        public List<CsvLine> GetCsvLines()
        {
            return _csv;
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
                        if (line.GetExecTimeOfLine(selector) != 0)
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
        public object GetTimeline()
        {
            DateTime initTime = this._csv[0].date;
            DateTime latestTime = this._csv[this._csv.Count - 1].date;

            // Berechne die Differenz zwischen initTime und latestTime
            TimeSpan difference = initTime - latestTime;

            return new { initTime, latestTime, difference };
        }
        public object GetIndexOfSearch(string subStr)
        {
            List<CsvLine> result = new List<CsvLine>();
            var indexOfres = 0;

            foreach (CsvLine str in this._csv)
            {
                // search subStr in every line and note the index
                if (str.data.ToLower().Contains(subStr.ToLower()))
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
                List<CsvLine> _res = new List<CsvLine>();

                bool isBetween = false;

                foreach (CsvLine str in this._csv)
                {
                    if (str.data.ToLower().Contains(firstSub.ToLower()) && !isBetween)
                    {
                        isBetween = true;
                    }

                    if (isBetween)
                    {
                        _res.Add(str);
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

            List<List<CsvLine>> _res = new List<List<CsvLine>>();
            List<CsvLine> _temp = new List<CsvLine>();

            bool isBetween = false;


            //if (!this.CheckDBData(this._csv[0]))
            //{
            foreach (CsvLine str in this._csv)
            {

                if (str.data.ToLower().Contains(_testdata_startStr.ToLower()) && !isBetween)
                {
                    isBetween = true;
                    _temp = new List<CsvLine>();
                }

                if (isBetween)
                {
                    dynamic item = this.GetIndexOfSearch(str.data);

                    _temp.Add(str);
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

        public void CreateHistory(){
            HistoryFileData history = new HistoryFileData();

            history.id = ""; // TODO
            history.name = Path.GetFileName(_file_path);

            history.values.testData.testCount = 0; // TODO
            history.values.testData.testPass = 0; // TODO
            history.values.testData.testFail = 0; // TODO
            history.values.testData.testPassRate = 100 / history.values.testData.testCount * history.values.testData.testPass; 

            /*string[] tests = new string[4]; // per test found in file, new test-object
            // test-object array/list? 
            tests[0]; // name
            tests[1]; // result
            tests[2]; // time 
            tests[3]; // vals=motvoltage,etc */

            string date = this._creation_time.ToString("yyyy-MM-dd");
            string time = this._creation_time.ToString("HH:mm:ss"); 
            string type = Path.GetExtension(_file_path); // csv = MotTestLog ; others = undefined
            string size = $"{((double)this._file.Length / (1024 * 1024)):F2}MB";
            string status; 
            if (testPass + testFail == testCount){ 
                status = "Finished";
            }
            else{
                status = "Airing";
            } 
            string pTime; // TODO ??
            float progress; // TODO ??
            string message = status; // difference to status??
        }
    }
}