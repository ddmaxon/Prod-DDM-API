namespace Prod_DDM_API.Data
{
    public static class Config
    {
        /*
         CSV Data Config
         */
        public static string[] CSVDataConfig = { "sCw_MotCurrent<mA>", "sCw_MotVoltage<V>", "sCw_MotSpeed<rpm>", "sCw_MotCurrent<mA>", "_MotComAngleOffset<degEl>", "_MotComAngleDelay<degEl>" };
        public static string[] CSVDataKeyConfig = { "DB", "I", "Q 'DB'", "Q 'SM'", "Q 'HW'" };

        /*
         DB Config
         */
        public static string DB_HOST = "vpn.wicki.sbs";
        public static string DB_DATABASE = "prod_ddm_db";
        public static string DB_USER_NAME = "root";
        public static string DB_USER_PASSWORD = "password";
    }
}
