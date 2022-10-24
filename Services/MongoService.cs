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
        private readonly IMongoDatabase _database;

        public MongoService(IOptions<FlightBookingDatabaseSettings> flightBookingDatabaseSettings)
        {
            var mongoClient = new MongoClient(flightBookingDatabaseSettings.Value.ConnectionString);

            _database = mongoClient.GetDatabase(
                flightBookingDatabaseSettings.Value.DatabaseName);

            _airplanesCollection = _database.GetCollection<Airplane>(
                flightBookingDatabaseSettings.Value.AirplanesCollectionName);
            _passengersCollection = _database.GetCollection<Passenger>(
                flightBookingDatabaseSettings.Value.PassengersCollectionName);
        }

        public async Task<List<Airplane>> GetAllAirplanesAsync() =>
            await _airplanesCollection.Find(airplane => true).ToListAsync();

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
            var newAirplane = new Airplane()
            {
                Name = airplaneName,
                Capacity = capacity,
                FlightTime = flightTime,
                Destination = destination
            };

            _airplanesCollection.InsertOne(newAirplane);
            return _airplanesCollection.Find(airplane => airplane.Id == newAirplane.Id).Any();
        }

        public DateTime? ParsedDate(string time)
        {
            return DateTime.ParseExact(time, "MM/dd/yyyy HH:mm:ss", null);
        }

        public async Task<Dictionary<int, int>> GroupTickets()
        {
            PipelineDefinition<Airplane, BsonDocument> pipeline = new BsonDocument[]
            {
                new BsonDocument("$unwind",
                    new BsonDocument("path", "$Tickets")),
                        new BsonDocument("$group",
                            new BsonDocument
                            {
                                { "_id", "$Tickets.Price" },
                                { "Count", new BsonDocument("$sum", 1) }
                            })
            };

            var results = await _airplanesCollection.Aggregate(pipeline).ToListAsync();

            var pricesCounts = new Dictionary<int, int>();
            foreach(var result in results)
            {
                pricesCounts.Add(result.AsBsonDocument[0].ToInt32(), result.AsBsonDocument[1].ToInt32());
            }

            return pricesCounts;
        }

        public async Task<Dictionary<int, int>> GroupTicketsMapReduce()
        {
            string map = @"
                function() {
                    var ticket = this;
                    emit(ticket.Price, { count: 1 });
                }";

            string reduce = @"        
                function(key, values) {
                    var result = {count: 0};

                    values.forEach(function(value){               
                        result.count += value.count;
                    });

                    return result;
                }";

            var options = new MapReduceOptions<Airplane, KeyValuePair<int, int>>();
            options.OutputOptions = MapReduceOutputOptions.Inline;

            var results = _airplanesCollection.MapReduceAsync(map, reduce, options).Result.ToList();

            var pricesCounts = new Dictionary<int, int>();
            foreach (var result in results)
            {
                pricesCounts.Add(result.Key, result.Value);
            }

            return pricesCounts;
        }
    }
}
