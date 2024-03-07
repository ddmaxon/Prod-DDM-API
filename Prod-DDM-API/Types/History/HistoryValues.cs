namespace Prod_DDM_API.Types.History
{
    public class HistoryValues
    {
        public HistoryTestData testData { get; set; }
        public DateOnly date { get; set; }
        public TimeOnly time  { get; set; }
        public string type { get; set; }
        public string size { get; set; }
        public HistoryProcess process { get; set; }
    }
}