using Microsoft.Extensions.Options;
using Mongo.Infrastructure.Models;
using Mongo.WebUI.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.WebUI.Services
{
    public class MongoService
    {
        private readonly IMongoCollection<Airplane> _airplanesCollection;
        private readonly IMongoCollection<Passenger> _passengersCollection;

        public MongoService(IOptions<FlightBookingDatabaseSettings> flightBookingDatabaseSettings)
        {
            var mongoClient = new MongoClient(flightBookingDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                flightBookingDatabaseSettings.Value.DatabaseName);

            _airplanesCollection = mongoDatabase.GetCollection<Airplane>(
                flightBookingDatabaseSettings.Value.AirplanesCollectionName);
            _passengersCollection = mongoDatabase.GetCollection<Passenger>(
                flightBookingDatabaseSettings.Value.PassengersCollectionName);
        }

        public async Task<List<Airplane>> GetAllAirplanesAsync() =>
            await _airplanesCollection.Find(airplane => airplane.Tickets.Any()).ToListAsync();

        public async Task<List<Ticket>> GetAllTicketsFromAllAirplanesAsync()
        {
            var airplanesWithTickets = await GetAllAirplanesAsync();
            return airplanesWithTickets.SelectMany(airplane => airplane.Tickets).ToList();
        }

        public async Task<bool> BookTicketAsync(string destination, string passengerName, string passengerSurname, int seatClass)
        {
            var airplane = await GetAvailableAirplaneByDestination(destination);

            if (airplane == null)
            {
                return false;
            }

            var passenger = await AddPassengerAsync(passengerName, passengerSurname);
            var result = await AddTicketAsync(passenger.Id, seatClass, airplane);

            if (!result)
            {
                return false;
            }

            return await ReduceCapacityAsync(airplane);
        }

        public async Task<Airplane?> GetAvailableAirplaneByDestination(string destination)
        {
            var airplanesToDestination = await _airplanesCollection.Find(airplane => airplane.Destination == destination && airplane.Capacity > 0).ToListAsync();

            if (airplanesToDestination.Count != 0)
            {
                return airplanesToDestination.OrderBy(x => x.FlightTime).First();
            }

            return null;
        }
        private async Task<Passenger> AddPassengerAsync(string passengerName, string passengerSurname)
        {
            var passenger = new Passenger()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = passengerName,
                Surname = passengerSurname
            };

            await _passengersCollection.InsertOneAsync(passenger);
            return passenger;
        }

        private async Task<bool> AddTicketAsync(string passengerId, int seatClass, Airplane selectedAirplane)
        {
            var price = PriceMapper.MapPrice(seatClass);

            var filter = Builders<Airplane>.Filter.Eq(nameof(Airplane.Id), selectedAirplane.Id);
            var update = Builders<Airplane>.Update.Push(nameof(Airplane.Tickets), new Ticket
            {
                Id = ObjectId.GenerateNewId().ToString(),
                TicketClass = seatClass,
                Price = price,
                Destination = selectedAirplane.Destination,
                PassengerId = passengerId
            });

            var result = await _airplanesCollection.UpdateOneAsync(filter, update);

            return result.MatchedCount == 1 && result.ModifiedCount == 1;
        }

        private async Task<bool> ReduceCapacityAsync(Airplane airplane)
        {
            var filter = Builders<Airplane>.Filter.Eq(nameof(Airplane.Id), airplane.Id);
            var update = Builders<Airplane>.Update.Set(nameof(Airplane.Capacity), airplane.Capacity - 1);

            var result = await _airplanesCollection.UpdateOneAsync(filter, update);

            return result.MatchedCount == 1 && result.ModifiedCount == 1;
        }

        public bool CreateAirplane(string airplaneName, int capacity, DateTime flightTime, string destination)
        {
            var airplane = new Airplane()
            {
                Name = airplaneName,
                Capacity = capacity,
                FlightTime = flightTime,
                Destination = destination
            };

            _airplanesCollection.InsertOne(airplane);
            return _airplanesCollection.Find(airplane => airplane.Id == airplane.Id).Any();
        }

        public DateTime? ParsedDate(DateTime time)
        {
            if (time > DateTime.Now)
            {
                return DateTime.Parse($"{time.Month}/{time.Day}/{time.Year} {time.Hour}:{time.Minute}:{time.Second}");
            }

            return null;
        }
    }
}
