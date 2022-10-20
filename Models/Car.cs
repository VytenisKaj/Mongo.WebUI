using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongo.Infrastructure.Models
{
    public class Car
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Manufacturer { get; set; }

        public bool IsAvailable { get; set; }

    }
}
