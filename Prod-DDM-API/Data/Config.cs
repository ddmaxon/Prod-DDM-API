namespace Prod_DDM_API.Data
{
    public static class Config
    {
        /*
         CSV Data Config
         */
        public static readonly string[] CSVDataConfig = { "MotCurrent<mA>", "MotVoltage<V>", "MotSpeed<rpm>", "MotCurrent<mA>", "MotComAngleOffset<degEl>", "MotComAngleDelay<degEl>" };
        public static readonly string[] CSVDataKeyConfig = { "DB", "I", "Q 'DB'", "Q 'SM'", "Q 'HW'" };

        /*
         DB Config
         */
        public static readonly string DB_HOST = "vpn.wicki.sbs";
        public static readonly string DB_DATABASE = "entw_ddm_db";
        public static readonly string DB_USER_NAME = "root";
        public static readonly string DB_USER_PASSWORD = "L0y1/4/WFC3{&r%j@U>H";
    }
}
