using MySql.Data.MySqlClient;
using Mysqlx.Resultset;
using Prod_DDM_API.types.Sql;
using Prod_DDM_API.Types;
using System.Xml.Linq;
using Prod_DDM_API.Types.History;
using static System.Collections.Specialized.BitVector32;

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
                string path = fc.GetFileInfo().FullName;

                // Get the count of all Lines
                string rCount = Convert.ToString(fc.GetCsvLines().Count);

                // Get the count of all tests
                dynamic tests = fc.GetAllTests();
                string tCount = Convert.ToString(tests.testCount);

                //Connect to the DB
                this._db.ConnectDb();

                // Execute the insert statement
                this._db.execCUD($"INSERT INTO ddm_files (name, _creation_time, _file_path, _rows_count, _tests_count) VALUES ('{name}', '{cTime}', '{path}', '{rCount}', '{tCount}')");

                List<CsvLine> rows = fc.GetCsvLines();
                List<SqlFileOutput> parent = GetParentFiles(name);

                this.InsertRows(rows, int.Parse(parent[0].id), true);

                //Disconnect DB
                this._db.DisconnectDb();

                // Return a response
                return this.GetOutput(200, $"File {name} successfully inserted into the db!");

            }
            catch (Exception err)
            {
                // Return a response
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
                this._db.execCUD($"INSERT INTO ddm_rows (fileid, date, section, exec_file, message, _index) VALUES ({fileid}, '{date}', '{section}', '{exec_file}', '{message}', {_index})");

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
                this._db.execCUD($"INSERT INTO ddm_rows (fileid, date, section, exec_file, message, _index) VALUES {string.Join(", ", values)};");

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
                    output._file_path = reader["_file_path"] + "";
                    output._creation_time = reader["_creation_time"] + "";
                    output._tests_count = reader["_tests_count"] + "";
                    output._rows_count = reader["_rows_count"] + "";

                    files.Add(output);
                };

                reader.Close();

                //Disconnect DB
                this._db.DisconnectDb();

                return this.GetOutput(200, "Files successfully read!", files, 0);
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

                return this.GetOutput(200, "Files successfully read!", files, 0);
            }
            catch (Exception err)
            {
                return this.GetOutput(500, err.Message);
            }
        }

        public int GetTestPassed(int fileId)
        {
            //Connect to DB
            this._db.ConnectDb();

            MySqlDataReader reader = this._db.execR($"SELECT COUNT(*) FROM ddm_tests WHERE result LIKE '%Pass%' AND fileid = {fileId}"); 

            int passedTests = 0;

            //Read the data and count the rows containing "failed"
            if (reader.Read()){
                passedTests = reader.GetInt32(0);
            }

            reader.Close();

            //Disconnect DB
            this._db.DisconnectDb();

            return passedTests;
        }
        public int GetTestFailed(int fileId)
        {
            //Connect to DB
            this._db.ConnectDb();

            MySqlDataReader reader = this._db.execR($"SELECT COUNT(*) FROM ddm_tests WHERE result LIKE '%Failed%' AND fileid = {fileId}"); 

            int failedTests = 0;

            //Read the data and count the rows containing "failed"
            if (reader.Read()){
                failedTests = reader.GetInt32(0);
            }

            reader.Close();

            //Disconnect DB
            this._db.DisconnectDb();

            return failedTests;
        }
        public List<HistoryTests> GetTestInfo(int fileId)
        {
            //Connect to DB
            this._db.ConnectDb();

            MySqlDataReader reader = this._db.execR($"SELECT * FROM intg_ddm_db WHERE fileid = {fileId}");

            List<HistoryTests> tests = new List<HistoryTests>();
            
            // Read data and store in a list
            while (reader.Read())
            {
                HistoryTests test = new HistoryTests();

                test.name = reader["name"] + "";
                string time = reader["processTime"] + "";
                test.time = TimeSpan.ParseExact(time, "HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                test.result = reader["result"] + "";
                
                tests.Add(test);
            }
            
            reader.Close();

            //Disconnect DB
            this._db.DisconnectDb();

            return tests;
        }
    }
}
