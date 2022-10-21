using MongoDB.Bson;

namespace Mongo.WebUI.Models
{
    public class Ticket
    {
        public string Id { get; set; }
        public string? PassengerName { get; set; }
        public string? PassengerSurname { get; set; }
        public int TicketClass { get; set; }

        // Id is not generated for embedded objects, doing it manually
        public Ticket() { Id = ObjectId.GenerateNewId().ToString(); }
    }
}
