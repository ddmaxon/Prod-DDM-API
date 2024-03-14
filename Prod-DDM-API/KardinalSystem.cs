using MongoDB.Bson;
using Prod_DDM_API.Classes;
using Prod_DDM_API.Classes.Db;
using Prod_DDM_API.Data;
using Prod_DDM_API.Types;
using Prod_DDM_API.Types.History;
using Prod_DDM_API.Types.Kardinal;
using Prod_DDM_API.types.Sql;

namespace Prod_DDM_API
{
    public class KardinalSystem
    {
        public KardinalSystem()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            // Initialize the system
        }

        private bool HasKey(string key, dynamic data)
        {
            try
            {
                // Attempt to access the key passed as parameter
                var value = data[key];
                // If the access is successful, it means the key exists
                return true;
            }
            catch (Exception)
            {
                // If an error occurs, it means the key doesn't exist
                return false;
            }
        }

        private KardinalOutput ExecFunc(Func<dynamic> method, string methodName = "Kardinal standard function")
        {
            try
            {
                dynamic response = method();


                if (this.HasKey("status", response))
                {
                    if (response.status != 200)
                    {
                        throw new Exception(response.message + " | " + response.funcName + " => " + response.status);
                    }
                }

                return new KardinalOutput
                {
                    message = "Successfully executed",
                    funcName = methodName,
                    isSuccessfull = true,
                    data = response,
                    status = 200
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new KardinalOutput
                {
                    message = e.Message,
                    funcName = methodName,
                    isSuccessfull = false,
                    data = null,
                    status = 500
                };
            }
        }

        public KardinalOutput GetFunctionExecution(string method, string[] mParam = null)
        {
            try
            {
                switch (method)
                {
                    case "TestInsertFile":
                        // Test the insert file
                        return this.ExecFunc(() =>
                        {
                            FileController fc = new FileController("./Data/Csv/testdata.csv");

                            return fc.testInsert();
                        }, "TestInsertFile");

                    case "History":
                        // Get the history
                        break;

                    case "HistoryID":
                        // Get the history by id

                        return this.ExecFunc(() =>
                        {
                            List<HistoryFileData> f = new List<HistoryFileData>();
                            Action<string[]> method;
                            method = (string[] mParam) =>
                            {
                                // Get the file by id
                                StorageOutput db_file =
                                    this.SearchSQL($"SELECT * FROM ddm_files WHERE id = {mParam[0]}");

                                // Get all tests of file
                                Console.WriteLine(db_file.ToJson());
                                FileController loader =
                                    new FileController("Data/Csv/" + db_file.data[0]["name"].ToString());

                                StorageOutput db_tests =
                                    this.SearchSQL($"SELECT * FROM ddm_tests WHERE fid = {mParam[0]}");
                                StorageOutput db_tests_vls =
                                    this.SearchSQL($"SELECT * FROM ddm_tests_vls WHERE fid = {mParam[0]}");

                                Console.WriteLine(db_tests_vls.ToJson());

                                if (!db_tests.isSuccessfull || !db_tests_vls.isSuccessfull)
                                {
                                    throw new KeyNotFoundException(
                                        "Some data not found in the database or the database is not available!");
                                }

                                List<HistoryTests> tests = new List<HistoryTests>();
                                List<HistoryTestsValue> testValues;
                                foreach (dynamic test in db_tests.data)
                                {
                                    testValues = new List<HistoryTestsValue>();
                                    foreach (dynamic test_vl in db_tests_vls.data)
                                    {
                                        if (test["id"].ToString() == test_vl["tid"].ToString())
                                        {
                                            HistoryTestsValue vls = new HistoryTestsValue();
                                            HistoryTestsValueData vlsData = new HistoryTestsValueData();

                                            vlsData.min = test_vl["vls_min"].ToString();
                                            vlsData.max = test_vl["vls_max"].ToString();
                                            vlsData.avg = test_vl["vls_avg"].ToString();

                                            vls.id = test_vl["id"].ToString();
                                            vls.key = test_vl["key"].ToString();
                                            vls.result = test_vl["res"].ToString();
                                            vls.value = vlsData;

                                            testValues.Add(vls);
                                        }
                                    }

                                    HistoryTests t = new HistoryTests();
                                    t.name = test["name"].ToString();
                                    t.result = test["res"].ToString();
                                    t.time = TimeSpan.Parse(test["progresstime"]);
                                    t.vals = testValues;

                                    tests.Add(t);
                                }

                                HistoryTestData tData = new HistoryTestData();

                                //Get the progress
                                HistoryFileData fData = new HistoryFileData();
                                HistoryValues vData = new HistoryValues();

                                tData.tests = tests;

                                Console.WriteLine(tests.ToJson());

                                tData.testCount = tData.tests.Count;
                                Console.WriteLine(tData.testCount);
                                tData.testFail = tData.tests.Count(obj =>
                                    obj.GetType().GetProperty("result")?.GetValue(obj)?.ToString()?.ToLower() ==
                                    "failed");
                                tData.testPass = tData.tests.Count(obj =>
                                    obj.GetType().GetProperty("result")?.GetValue(obj)?.ToString()?.ToLower() ==
                                    "pass");
                                tData.testPassRate = tData.testPass == tData.testCount
                                    ? 100
                                    // ReSharper disable once PossibleLossOfFraction
                                    : (tData.testPass / tData.testCount) * 100;

                                fData.name = db_file.data[0]["name"].ToString();
                                fData.id = db_file.data[0]["id"].ToString();
                                vData.testData = tData;
                                DateTime fileDate = loader.GetCreationTime();

                                vData.date = new DateOnly(fileDate.Year, fileDate.Month, fileDate.Day);
                                vData.time = new TimeOnly(fileDate.Hour, fileDate.Minute, fileDate.Second);

                                //Get size of file
                                long size = loader.GetFileInfo().Length;

                                //Change to MB
                                vData.size = Convert.ToString((size / 1024) / 1024) + "MB";
                                vData.type = "CSV (MotTestLog)";
                                vData.process = new HistoryProcess
                                {
                                    // TODO: Logic implementation
                                    message = "Finished",
                                    progress = 100,
                                    status = "Finished",
                                    time = new TimeSpan(59000)
                                };

                                fData.values = vData;

                                f.Add(fData);
                            };
                            
                            method(mParam);
                            method(new string[] {"2"});

                            return f;
                        }, "HistoryID");


                        break;

                    case "Timeline":
                        // Get the timeline
                        return this.ExecFunc(() =>
                        {
                            FileController loader = new FileController("./Data/Csv/testdata2.csv");

                            List<object> list = new List<object>();

                            object timeline = loader.GetTimeline();

                            object all =
                                loader.GetExecutionTimeWithSelectors(loader.GetExecutionTime(loader.GetCsvLines())
                                    .ToArray());

                            foreach (var key in Config.CSVDataKeyConfig)
                            {
                                list.Add(new
                                {
                                    key,
                                    value = loader.GetExecutionTimeWithSelectors(
                                        loader.GetExecutionTime(loader.GetCsvLines(), key).ToArray())
                                });
                            }

                            return new
                            {
                                timeline,
                                all,
                                sorted = list
                            };
                        }, "Timeline");

                    case "GetValues":
                        return this.ExecFunc(() =>
                        {
                            FileController loader = new FileController("./Data/Csv/testdata2.csv");
                            dynamic data = loader.GetTestsWithValues();

                            return data;
                        }, "GetValues");

                    default:
                        return new KardinalOutput
                        {
                            message = "Method not found",
                            funcName = method,
                            isSuccessfull = false,
                            data = null,
                            status = 404
                        };
                }
            }
            catch (Exception e)
            {
                return new KardinalOutput
                {
                    message = e.Message,
                    funcName = method,
                    isSuccessfull = false,
                    data = null,
                    status = 500
                };
            }

            return new KardinalOutput
            {
                message = "Something went wrong in Kardinal ExecFunction!",
                funcName = method,
                isSuccessfull = false,
                data = null,
                status = 500
            };
        }

        private StorageOutput SearchSQL(string query)
        {
            StorageController db = new StorageController();

            return db.SearchKardinalQuery(query);
        }
    }
}