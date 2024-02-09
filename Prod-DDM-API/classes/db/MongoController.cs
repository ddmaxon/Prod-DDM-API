using MongoDB.Driver;
using MongoDB.Bson;
using Prod_DDM_API.Data;


namespace Prod_DDM_API.Classes.Db 
{
    public class MongoController
    {
        private string mongodb_uri = Config.MONGODB_URI;
        private string mongodb_db = Config.MONGODB_DB;
        private string mongodb_collection = Config.MONGODB_COLLECTION;

        private dynamic _db;

        public MongoController()
        {
            this.mongodb_uri = Config.MONGODB_URI;
            this.mongodb_db = Config.MONGODB_DB;
            this.mongodb_collection = Config.MONGODB_COLLECTION;

            // Connect to the DB
            this._db = this.connect();
        }


        private dynamic connect()
        {
            Console.WriteLine(this.mongodb_uri);
            var connectionString = this.mongodb_uri;
            if (connectionString == null)
            {
                throw new InvalidDataException("You must set your 'MONGODB_URI' environment variable.");
            }
            var client = new MongoClient(connectionString);
            var collection = client.GetDatabase(this.mongodb_db).GetCollection<BsonDocument>(this.mongodb_collection);

            return collection;
        }

        public dynamic Search(object param)
        {
            return "Hallo Welt";
        }
    }
}
