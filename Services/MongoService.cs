using Microsoft.Extensions.Options;
using Mongo.Infrastructure.Models;
using Mongo.WebUI.Models;
using MongoDB.Driver;

namespace Mongo.WebUI.Services
{
    public class MongoService
    {
        private readonly IMongoCollection<Airplane> _airplaneCollection;
        private readonly IMongoCollection<Passenger> _passengerCollection;

        public MongoService(
            IOptions<FlightBookingDatabaseSettings> flightBookingDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                flightBookingDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                flightBookingDatabaseSettings.Value.DatabaseName);

            _airplaneCollection = mongoDatabase.GetCollection<Airplane>(
                flightBookingDatabaseSettings.Value.AirplanesCollectionName);
            _passengerCollection = mongoDatabase.GetCollection<Passenger>(
                flightBookingDatabaseSettings.Value.PassengersCollectionName);
        }

        public async Task<List<Airplane>> GetAsync() =>
            await _airplaneCollection.Find(_ => true).ToListAsync();

        public async Task<Airplane?> GetAsync(string id) =>
            await _airplaneCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Airplane newAirplane) =>
            await _airplaneCollection.InsertOneAsync(newAirplane);

        public async Task UpdateAsync(string id, Airplane updatedAirplane) =>
            await _airplaneCollection.ReplaceOneAsync(x => x.Id == id, updatedAirplane);

        public async Task RemoveAsync(string id) =>
            await _airplaneCollection.DeleteOneAsync(x => x.Id == id);
    }
}
