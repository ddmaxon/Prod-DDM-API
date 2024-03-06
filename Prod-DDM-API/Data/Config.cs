namespace Prod_DDM_API.Data
{
    public static class Config
    {
        /*
         CSV Data Config
         */
        public static readonly string[] CSVDataConfig = { "sCw_MotCurrent<mA>", "sCw_MotVoltage<V>", "sCw_MotSpeed<rpm>", "sCw_MotCurrent<mA>", "_MotComAngleOffset<degEl>", "_MotComAngleDelay<degEl>" };
        public static readonly string[] CSVDataKeyConfig = { "DB", "I", "Q 'DB'", "Q 'SM'", "Q 'HW'" };

        /*
         DB Config
         */
        public static readonly string DB_HOST = "vpn.wicki.sbs";
        public static readonly string DB_DATABASE = "prod_ddm_db";
        public static readonly string DB_USER_NAME = "root";
        public static readonly string DB_USER_PASSWORD = "password";
    }
}
