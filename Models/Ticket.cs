using MongoDB.Bson;

namespace Mongo.WebUI.Models
{
    public class Ticket
    {
        public string Id { get; set; }
        public int TicketClass { get; set; }
        public int Price { get; set; }
        public string Destination { get; set; }
        public string PassengerId { get; set; }

        // Id is not generated for embedded objects, doing it manually
        public Ticket() { Id = ObjectId.GenerateNewId().ToString(); }
    }
}
