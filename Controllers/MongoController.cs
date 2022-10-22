using Microsoft.AspNetCore.Mvc;
using Mongo.WebUI.Models;
using Mongo.WebUI.Services;
using Newtonsoft.Json;

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

        [HttpGet("/allAirplanes")]
        public async Task<List<Airplane>> GetAllAirplanesAsync()
        {
            return await _mongoService.GetAllAirplanesAsync();
        }

        [HttpGet("/allTickets")]
        public async Task<List<Ticket>> GetAllTicketsAsync()
        {
            return await _mongoService.GetAllTicketsFromAllAirplanesAsync();
        }

        [ProducesResponseType(typeof(IActionResult), 200)]
        [ProducesResponseType(typeof(BadRequestObjectResult), 404)]
        [HttpGet("availableFlight/{destination}")]
        public async Task<IActionResult> GetAvailableFlightToDestinationAsync(string destination)
        {
            var airplane = await _mongoService.GetAvailableAirplaneByDestination(destination);

            if (airplane == null)
            {
                return new BadRequestObjectResult("No airplanes found to desired destination.");
            }

            var result = new List<string>()
            {
                $"Airline: {airplane.Name}",
                $"Destination: {airplane.Destination}",
                $"Departure: {airplane.FlightTime}",
                $"Capacity: {airplane.Capacity}",
                $"Flight ID: {airplane.Id}",
            };

            return Content(JsonConvert.SerializeObject(result));
        }

        [ProducesResponseType(typeof(IActionResult), 200)]
        [ProducesResponseType(typeof(BadRequestObjectResult), 400)]
        [HttpPost("bookTicket/{destination}/{passengerName}/{passengerSurname}/{seatClass}")]
        public async Task<IActionResult> BookTicketAsync(string destination, string passengerName, string passengerSurname, int seatClass)
        {
            var result = await _mongoService.BookTicketAsync(destination, passengerName, passengerSurname, seatClass);

            if (result)
            {
                return Content($"Ticket for {passengerName} {passengerSurname} was booked successfully. Pack your bags!");
            }

            return new BadRequestObjectResult($"Unable to book a ticket. Check if destination is available. Or maybe flight is fully booked? Could be, no flights to {destination} for you then:)");
        }

        [ProducesResponseType(typeof(IActionResult), 200)]
        [ProducesResponseType(typeof(BadRequestObjectResult), 400)]
        [HttpPost("addAirplane")]
        public IActionResult AddAirplane([FromBody] Flight flight)
        {
            var parsedDate = _mongoService.ParsedDate((DateTime)flight.FlightTime);

            if (parsedDate == null)
            {
                return new BadRequestObjectResult("Unavailable date selected. Select future date for new flight.");
            }

            var result = _mongoService.CreateAirplane(flight.Name, (int)flight.Capacity, (DateTime)parsedDate, flight.Destination);

            if (!result)
            {
                return new BadRequestObjectResult("Error occurred while adding a new airplane.");
            }

            return Content($"Airplane to destination {flight.Destination} was added successfully!");
        }
    }
}