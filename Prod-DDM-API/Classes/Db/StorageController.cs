using MongoDB.Bson;
using MySql.Data.MySqlClient;
using Prod_DDM_API.types.Sql;
using Prod_DDM_API.Types;
using Prod_DDM_API.Types.History;

namespace Prod_DDM_API.Classes.Db
{
    public class StorageController
    {
        private Storage _db;

        public StorageController()
        {
            this._db = new Storage();
        }

        private StorageOutput GetOutput(int state, string message = "Successfully executed", dynamic data = null, int affected = 1)
        {
            // Create a output / response
            StorageOutput result = new StorageOutput();
            bool isSuccessfull = true;

            if (state != 200)
            {
                isSuccessfull = false;
                affected = 0;

                if (message == "Successfully executed")
                {
                    message = "Something went wrong!";
                }
            }

            result.isSuccessfull = isSuccessfull;
            result.affected = affected;
            result.message = message;
            result.data = data;

            return result;
        }
        public StorageOutput InsertFile(FileController fc)
        {
            try
            {
                // Get the name
                string name = fc.GetFileInfo().Name;

                // Get the creation time
                string cTime = Convert.ToString(fc.GetCreationTime());

                // Get the path
                FileInfo f = fc.GetFileInfo();
                string path = Path.GetFullPath(f.Name);

                // Get the count of all Lines
                string rCount = Convert.ToString(fc.GetCsvLines().Count);

                // Get the count of all tests
                List<HistoryTests> tests = fc.GetTestsWithValues();
                string tCount = Convert.ToString(tests.Count);
                
                // Get the size of the file
                float size = (float)((f.Length / 1024) / 1024);

                // Get the passCount of all tests
                int testFail = tests.Count(obj => obj.GetType().GetProperty("result")?.GetValue(obj)?.ToString()?.ToLower() == "failed");
                int testPass = tests.Count(obj => obj.GetType().GetProperty("result")?.GetValue(obj)?.ToString()?.ToLower() == "pass");                
                
                Console.WriteLine(tests.ToJson());
                
                //Connect to the DB
                this._db.ConnectDb();
                
                // Execute the insert statement
                this._db.execCUD($"INSERT INTO ddm_files (name, _creation_time, _filepath, _rowcount, _testcount, _testpasscount, _testfailcount, _size) " +
                                       $"VALUES ('{name}', '{cTime}', '{path}', {rCount}, {tCount}, {testPass}, {testFail}, {size})");

                List<CsvLine> rows = fc.GetCsvLines();
                List<SqlFileOutput> parent = GetParentFiles(name);
                
                Console.WriteLine(parent.ToJson());

                this.InsertRows(rows, int.Parse(parent[0].id), true);

                this.InsertTests(fc, int.Parse(parent[0].id), tests);

                //Disconnect DB
                this._db.DisconnectDb();

                // Return a response
                return this.GetOutput(200, $"File {name} successfully inserted into the db!");

            }
            catch (Exception err)
            {
                Console.WriteLine(err);
                return this.GetOutput(500, err.Message);
            }
        }
        private List<SqlFileOutput> GetParentFiles(string name)
        {
            StorageOutput parent = GetFileByName(name);
            return (List<SqlFileOutput>)parent.data;
        }
        public StorageOutput InsertRow(CsvLine row)
        {
            try
            {
                // ID of parent CSV-File in the DB
                int fileid = int.Parse(row.parentCsv);

                // ID of test in the DB
                int testid = 0; // logic not implemented yet!

                // Date when row was written
                DateTime date = Convert.ToDateTime(row.SplitList()[0]);

                // Where was the logrow was
                string exec_file = row.SplitList()[1];

                // In what section is the row
                string section = row.SplitList()[2];

                // The message of the row
                string message = row.SplitList()[3];

                do
                {
                    message.Replace("'", "");
                } while (message.Contains("'"));


                // Index in the CSV-File
                int _index = row.index;

                //Connect to DB
                this._db.ConnectDb();

                // Execute the insert statemet
                this._db.execCUD($"INSERT INTO ddm_rows (fid, date, section, execfunction, msg, _index) VALUES ({fileid}, '{date}', '{section}', '{exec_file}', '{message}', {_index})");

                //Disconect DB
                this._db.DisconnectDb();

                // Return a response
                return this.GetOutput(200, $"Row {_index} successfully inserted into the db!");
            }
            catch (Exception err)
            {
                // Return a response
                return this.GetOutput(500, err.Message);
            }
        }
        public StorageOutput InsertRows(List<CsvLine> rows, int ParentID = 0, bool HasParentID = false)
        {
            try
            {
                //Connect to DB
                this._db.ConnectDb();

                List<string> values = new List<string>();

                foreach (CsvLine row in rows)
                {
                    // ID of parent CSV-File in the DB
                    int fileid = HasParentID ? ParentID : int.Parse(row.parentCsv);

                    // ID of test in the DB (Assuming some logic here)
                    int testid = DetermineTestID(row); // Implement your logic to determine testid

                    // Date when row was written
                    DateTime date = Convert.ToDateTime(row.SplitList()[0]);

                    // Where was the logrow was
                    string exec_file = row.SplitList()[1];
                    exec_file = exec_file.Replace("'", "");

                    // In what section is the row
                    string section = row.SplitList()[2];
                    section = section.Replace("'", "");

                    // The message of the row
                    string message = row.SplitList()[3];

                    // Remove single quotes from message
                    message = message.Replace("'", "");

                    // Index in the CSV-File
                    int _index = row.index;

                    values.Add($"({fileid}, '{date}', '{section}', '{exec_file}', '{message}', {_index})");
                }

                // Execute the insert statement
                this._db.execCUD($"INSERT INTO ddm_rows (fid, date, section, execfunction, msg, _index) VALUES {string.Join(", ", values)};");

                // Disconnect DB
                this._db.DisconnectDb();

                // Return a response
                return this.GetOutput(200, $"All files successfully inserted into the db!");
            }
            catch (Exception err)
            {
                // Return a response
                return this.GetOutput(500, err.Message);
            }
        }

        public StorageOutput InsertTests(FileController fc, int fileid, List<HistoryTests> tests = null)
        {
            //Get all tests with values
            if (tests == null)
            {
                tests = fc.GetTestsWithValues();
            }            
            
            //Connect to the DB
            this._db.ConnectDb();

            //Insert all tests
            List<string> values = new List<string>();
            List<string> secndVals = new List<string>();
            foreach (HistoryTests test in tests)
            {
                values.Add($"('{test.vals[0].id}', '{test.name}', '{test.result}', {fileid}, {test.vals[0].id}, '{test.time}')");
                foreach(HistoryTestsValue value in test.vals)
                {
                    while (value.id.Contains("'") || value.key.Contains("'") || value.value.min.Contains("'") || value.value.avg.Contains("'") || value.value.max.Contains("'"))
                    {
                        // Remove single quotes from message
                        value.key = value.key.Replace("'", "");
                        
                        value.value.min = value.value.min.Replace("'", "");
                        value.value.avg = value.value.avg.Replace("'", "");
                        value.value.max = value.value.max.Replace("'", "");
                    }
                    
                    secndVals.Add($"({fileid}, {value.id}, '{value.result}', '{value.key}', '{value.value.min}', '{value.value.avg}', '{value.value.max}')");
                }
                
            }
            this._db.execCUD($"INSERT INTO ddm_tests (id, name, res, fid, serialNumber,  progresstime) VALUES {string.Join(", ", values)}");

            this._db.execCUD($"INSERT INTO ddm_tests_vls (fid, tid, res, `key`, vls_min, vls_avg, vls_max) VALUES {string.Join(", ", secndVals)}");
            
            //Disconnect DB
            this._db.DisconnectDb();
            
            return this.GetOutput(200);
        }
        // Placeholder for determining testid
        private int DetermineTestID(CsvLine row)
        {
            // Implement your logic to determine testid
            // For now, return a placeholder value
            return 1;
        }
        public StorageOutput GetFileByName(string name)
        {
            try
            {
                //Connect to DB
                this._db.ConnectDb();

                MySqlDataReader reader = this._db.execR($"SELECT * FROM ddm_files where name like '%{name}%'");

                List<SqlFileOutput> files = new List<SqlFileOutput>();

                //Read the data and store them in the list
                while (reader.Read())
                {
                    SqlFileOutput output = new SqlFileOutput();

                    output.name = reader["name"] + "";
                    output.id = reader["id"] + "";
                    output._file_path = reader["_filepath"] + "";
                    output._creation_time = reader["_creation_time"] + "";
                    output._tests_count = reader["_testcount"] + "";
                    output._rows_count = reader["_rowcount"] + "";
                    output._testpass_count = reader["_testpasscount"] + "";
                    output._testfail_count = reader["_testfailcount"] + "";
                    output._size = reader["_size"] + "";


                    files.Add(output);
                };

                reader.Close();

                //Disconnect DB
                this._db.DisconnectDb();

                return this.GetOutput(200, "Files successfully readed!", files, 0);
            }
            catch (Exception err)
            {
                return this.GetOutput(500, err.Message);
            }
        }
        public StorageOutput GetFiles()
        {
            try
            {
                //Connect to DB
                this._db.ConnectDb();

                MySqlDataReader reader = this._db.execR("SELECT * FROM ddm_files");

                List<SqlFileOutput> files = new List<SqlFileOutput>();

                //Read the data and store them in the list
                while (reader.Read())
                {
                    SqlFileOutput output = new SqlFileOutput();

                    output.name = reader["name"] + "";
                    output.id = reader["id"] + "";
                    output._file_path = reader["_file_path"] + "";
                    output._creation_time = reader["_creation_time"] + "";
                    output._tests_count = reader["_tests_count"] + "";
                    output._rows_count = reader["_rows_count"] + "";

                    files.Add(output);
                };

                reader.Close();

                //Disconnect DB
                this._db.DisconnectDb();

                return this.GetOutput(200, "Files successfully readed!", files, 0);
            }
            catch (Exception err)
            {
                return this.GetOutput(500, err.Message);
            }
        }
        public StorageOutput SearchKardinalQuery(string kardinalQuery)
        {
            try
            {
                //Connect to DB
                this._db.ConnectDb();

                MySqlDataReader reader = this._db.execR(kardinalQuery);
                List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();

                dynamic output;

                //Read the data and store them in the list
                while (reader.Read())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();

                    // Iteriere über die Spalten und füge sie dem Dictionary hinzu
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);
                        object columnValue = reader.GetValue(i);
                        row[columnName] = columnValue;
                    }

                    resultList.Add(row);
                }

                if (resultList is List<SqlFileOutput>)
                {
                    output = new List<SqlFileOutput>();
                    
                    output = resultList;
                }

                reader.Close();

                //Disconnect DB
                this._db.DisconnectDb();

                return this.GetOutput(200, "Files successfully readed!", resultList, 0);
            }
            catch (Exception err)
            {
                return this.GetOutput(500, err.Message);
            }
        }
    }
}
