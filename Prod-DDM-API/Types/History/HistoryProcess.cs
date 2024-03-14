namespace Prod_DDM_API.Types.History
{
    public class HistoryProcess
    {
        public string status { get; set; }
        public TimeSpan time { get; set; }
        public float progress { get; set; }
        public string message { get; set; } 
    }
}