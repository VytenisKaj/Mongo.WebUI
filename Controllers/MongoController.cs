using Microsoft.AspNetCore.Mvc;
using Mongo.Infrastructure.Models;
using Mongo.WebUI.Services;

namespace Mongo.WebUI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MongoController : ControllerBase
    {

        private readonly MongoService _mongoService;

        public MongoController(MongoService mongoService)
        {
            _mongoService = mongoService;
        }


        [HttpGet]
        public async Task<List<Car>> Get() => await _mongoService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Car>> Get(string id)
        {
            var car = await _mongoService.GetAsync(id);

            if (car is null)
            {
                return NotFound();
            }

            return car;
        }


        [HttpPost]
        public async Task<IActionResult> Post(Car newCar)
        {
            await _mongoService.CreateAsync(newCar);

            return CreatedAtAction(nameof(Get), new { id = newCar.Id }, newCar);
        }
    }
}