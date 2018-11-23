using MongoDB.Bson.Serialization.Attributes;
namespace My_Profile
{
    public class User
    {

        [BsonId]
        public MongoDB.Bson.ObjectId Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }

    }
    public class Settings
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
    }
}