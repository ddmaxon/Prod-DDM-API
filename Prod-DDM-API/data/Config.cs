namespace Prod_DDM_API.Data
{
    public static class Config
    {
        /*
         MongoDB Config
         */
        public static string MONGODB_URI = "mongodb://localhost:27017";
        public static string MONGODB_DB = "DDM_STORAGE";
        public static string MONGODB_COLLECTION = "tests";


        /*
         CSV Data Config
         */
        public static string[] CSVDataConfig = { "sCw_MotCurrent<mA>", "sCw_MotVoltage<V>", "sCw_MotSpeed<rpm>", "sCw_MotCurrent<mA>", "_MotComAngleOffset<degEl>", "_MotComAngleDelay<degEl>" };
    }
}
