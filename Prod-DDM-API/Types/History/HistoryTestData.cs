namespace Prod_DDM_API.Types.History
{
    public class HistoryTestData
    {
       public int testCount { get; set; }
       public int testPass { get; set; }
       public int testFail { get; set; }
       public float testPassRate { get; set; }
       public List<HistoryTests> tests { get; set; }
    }
}