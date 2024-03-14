namespace Prod_DDM_API.types.Sql
{
    public class SqlFileOutput
    {
        public string id { get; set; }
        public string name { get; set; }
        public string _file_path { get; set; }
        public string _creation_time { get; set; }
        public string _rows_count { get; set; }
        public string _tests_count { get; set; }
        public string _size { get; set; }
        public string _testfail_count { get; set; }
        public string _testpass_count { get; set; }
    }
}
