using Prod_DDM_API.Types;
using Prod_DDM_API.Data;
//Add MySql Library
using MySql.Data.MySqlClient;

namespace Prod_DDM_API.Classes.Db
{
    public class Storage
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        public Storage()
        {
            this.Initialize();
        }

        //DB Logs
        private void WriteLog(int status, string query)
        {
            string logFilePath = "C:\\vsc\\_BLJ\\Prod-DDM-API\\Prod-DDM-API\\data\\Logs\\sql-log.log";
            string logDirectory = Path.GetDirectoryName(logFilePath);

            // Überprüfen und erstellen Sie das Protokollverzeichnis, falls es nicht existiert
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string statusText = status == 200 ? "Success" : status == 499 ? "Warning =>" : "!! Error !!";
            string logMessage = $"{DateTime.Now}: [{statusText}] - {query}";

            try
            {
                // Protokolle in die Datei schreiben
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(logMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(logMessage, ex);
            }
        }
        //DB error comparison
        private string CompareQueryXErr(string err, string query)
        {
            query = this.CheckQuerySize(query);
            
            return err + " | " + query;
        }
        //DB query size adjustment
        private string CheckQuerySize(string query)
        {
            int maxLenght = 100;
            if (query.Length > maxLenght)
            {
                //Resize query to 50 chars
                query = query.Substring(0, maxLenght) + "...";
            }

            return query;
        }
        //Initialize values
        private void Initialize()
        {
            this.server = Config.DB_HOST;
            this.database = Config.DB_DATABASE;
            this.uid = Config.DB_USER_NAME;
            this.password = Config.DB_USER_PASSWORD;

            string connectionString;
            connectionString = "SERVER=" + this.server + ";" + "DATABASE=" +
            this.database + ";" + "UID=" + this.uid + ";" + "PASSWORD=" + this.password + ";";

            this.connection = new MySqlConnection(connectionString);
        }
        //Public connect function
        public void ConnectDb()
        {
            if (!this.CheckConnection())
            {
                if (!this.OpenConnection()){
                    throw new DirectoryNotFoundException("Cannot connect to server.  Contact administrator");
                }
            }
        }
        //Public disconect function
        public void DisconnectDb()
        {
            if (this.CheckConnection())
            {
                if (!this.CloseConnection())
                {
                    throw new DirectoryNotFoundException("Cannot connect to server. Contact administrator");
                }
            }
        }
        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        throw new Exception("Cannot connect to server.  Contact administrator");

                    case 1045:
                        throw new Exception("Invalid username/password, please try again");
                }
                return false;
            }
        }
        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException err)
            {
                throw new Exception(err.Message);
            }
        }
        //Check connection to DB
        private bool CheckConnection()
        {
            if(this.connection.State == System.Data.ConnectionState.Open)
            {
                return true;
            }

            return false;
        }
        //All read execution
        public dynamic execR(string query)
        {
            try
            {
                //open connection
                if (this.CheckConnection())
                {
                    //create command and assign the query and connection from the constructor
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    //Execute command
                    dynamic data = cmd.ExecuteReader();

                    WriteLog(200, this.CheckQuerySize(query));

                    return data;
                }
                else
                {
                    throw new Exception("No connection to the database!");
                }
            }
            catch (Exception err)
            {
                WriteLog(500, this.CompareQueryXErr(err.Message, query));
                throw new Exception(err.Message);
            }
        }
        //All create (insert), update and delete executions
        public void execCUD(string query)
        {
            try
            {
                //open connection
                if (this.CheckConnection())
                {
                    //create command and assign the query and connection from the constructor
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    //Execute command
                    cmd.ExecuteNonQuery(); 

                    WriteLog(200, this.CheckQuerySize(query));
                }
                else
                {
                    throw new Exception("No connection to the database!");
                }
            }
            catch (Exception err)
            {
                WriteLog(500, this.CompareQueryXErr(err.Message, query));
                throw new Exception(err.Message);
            }
        }
    }
}
