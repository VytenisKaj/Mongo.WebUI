using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongo.WebUI.Models
{
    public class Airplane
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        public int Capacity { get; set; }
        public DateTime? FlightTime { get; set; }
        public string Destination { get; set; }

        public IEnumerable<Ticket> Tickets { get; set; }

        public Airplane()
        {
            Tickets = new List<Ticket>();
        }
    }
}
