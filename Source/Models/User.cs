using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace KnowledgeGraph.Models
{
    public class User
    {

        // [BsonId]
        // [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }

    }

}