namespace Mongo.WebUI.Models
{
    public class Flight
    {
        public string? Name { get; set; }
        public int? Capacity { get; set; }
        public DateTime? FlightTime { get; set; }
        public string? Destination { get; set; }

        public Flight() { }
    }
}
