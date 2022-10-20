using Microsoft.Extensions.Options;
using Mongo.Infrastructure.Models;
using MongoDB.Driver;

namespace Mongo.WebUI.Services
{
    public class MongoService
    {
        private readonly IMongoCollection<Car> _carsCollection;

        public MongoService(
            IOptions<CarRentalDatabaseSettings> carRentalDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                carRentalDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                carRentalDatabaseSettings.Value.DatabaseName);

            _carsCollection = mongoDatabase.GetCollection<Car>(
                carRentalDatabaseSettings.Value.CarsCollectionName);
        }

        public async Task<List<Car>> GetAsync() =>
            await _carsCollection.Find(_ => true).ToListAsync();

        public async Task<Car?> GetAsync(string id) =>
            await _carsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Car newCar) =>
            await _carsCollection.InsertOneAsync(newCar);

        public async Task UpdateAsync(string id, Car updatedCar) =>
            await _carsCollection.ReplaceOneAsync(x => x.Id == id, updatedCar);

        public async Task RemoveAsync(string id) =>
            await _carsCollection.DeleteOneAsync(x => x.Id == id);
    }
}
