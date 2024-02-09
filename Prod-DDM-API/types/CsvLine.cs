namespace Prod_DDM_API.types
{
    public class CsvLine
    {
        public string data { get; set; }
        public int index { get; set; }
        public string parentCsv { get; set; }

        public CsvLine(string data)
        {
            this.data = data;
        }

        public CsvLine(string data, int index)
        {
            this.data = data;
            this.index = index;
        }

        public CsvLine(string data, string parentCsv, int index)
        {
            this.index = index;
            this.parentCsv = parentCsv;
            this.data = data;
        }

        public List<string[]> SplitList(dynamic res)
        {
            List<string[]> splitLine = new List<string[]>();

            string[] tempArray = this.data.Split(';');
            splitLine.Add(tempArray);

            return splitLine;
        }
    }
}
