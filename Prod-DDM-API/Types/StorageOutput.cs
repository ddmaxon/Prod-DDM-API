namespace Prod_DDM_API.Types
{
    public class StorageOutput
    {
        public bool isSuccessfull { get; set; }
        public int affected { get; set; }
        public string message { get; set; }
        public dynamic data { get; set; }
    }
}
