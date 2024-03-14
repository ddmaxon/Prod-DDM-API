using System.Diagnostics;

namespace Prod_DDM_API.Types.Kardinal;

public class KardinalOutput
{
    public string message { get; set; }
    public bool isSuccessfull { get; set; }
    public dynamic data { get; set; }
    public int status { get; set; }
    public string funcName { get; set; }
    public Stopwatch execTime { get; set; }
}