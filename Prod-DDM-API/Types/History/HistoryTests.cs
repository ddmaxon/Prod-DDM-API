namespace Prod_DDM_API.Types.History
{
    public class HistoryTests
    {
        public string id { get; set; }
        public string name { get; set; }
        public string result { get; set; }
        public TimeSpan time { get; set; }
        public List<HistoryTestsValue> vals { get; set; }
    }
}