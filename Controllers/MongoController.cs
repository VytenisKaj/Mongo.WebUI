using Microsoft.AspNetCore.Mvc;
using Mongo.Infrastructure.Models;
using Mongo.WebUI.Models;
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

        [HttpGet("/allItems")]
        public async Task<List<Airplane>> Get() => await _mongoService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Airplane>> Get(string id)
        {
            var airplane = await _mongoService.GetAsync(id);

            if (airplane is null)
            {
                return NotFound();
            }

            return airplane;
        }


        [HttpPost("addAirplane/{name}/{capacity}/{destination}/{flightTime}")]
        public async Task<IActionResult> Post(string name, int capacity, string destination, string flightTime)
        {
            Airplane airplane = new()
            {
                Name = name,
                Destination = destination,
                FlightTime = DateTime.Parse(flightTime),
                Capacity = capacity
            };
            await _mongoService.CreateAsync(airplane);

            return CreatedAtAction(nameof(Get), new { id = airplane.Id }, airplane);
        }
    }
}