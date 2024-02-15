namespace Prod_DDM_API.Types
{
    public class CsvLine
    {
        public string data { get; set; }
        public int index { get; set; }
        public string parentCsv { get; set; }
        public DateTime date { get; set; }

        public CsvLine(string data)
        {
            this.data = data;

            this.init();
        }

        public CsvLine(string data, int index)
        {
            this.data = data;
            this.index = index;
            
            this.init();
        }

        public CsvLine(string data, string parentCsv, int index)
        {
            this.index = index;
            this.parentCsv = parentCsv;
            this.data = data;

            this.init();
        }

        private void init()
        {
            this.date = DateTime.Parse(this.SplitList()[0]);
        }

        public string[] SplitList()
        {
            return this.data.Split(';');
        }

        public double GetExecTimeOfLine(string selector = null)
        {
            // 11.07.2023 16:29:07.439;Datenbank.vi;Q 'DB';End: "DB_ErrorLog" [0.130 s] 
            string[] strArr = this.SplitList();
            if(selector == null)
            {
                return this.GetExec(strArr);
            }
            else if (strArr[2].ToLower() == selector.ToLower())
            {
                return this.GetExec(strArr);
            }

            return 0;
        }

        private double GetExec(string[] strArr)
        {
            Array.Reverse(strArr);

            // ["End: "DB_ErrorLog" [0.130 s]", "Q 'DB'", "Datenbank.vi", "11.07.2023 16:29:07.439"] 
            string[] exec_timeArr = strArr[0].Split(" ");
            Array.Reverse(exec_timeArr);

            if (exec_timeArr.Contains("s]"))
            {
                string exec_time = exec_timeArr[2];

                if (exec_timeArr[0].Contains("s]"))
                {
                    exec_time = exec_timeArr[1];
                }

                // [0.130 s]
                exec_time = exec_time.Replace("[", "");

                // 0.130
                if (double.TryParse(exec_time, out _))
                {
                    return double.Parse(exec_time);
                }
            }

            return 0;
        }
    }
}