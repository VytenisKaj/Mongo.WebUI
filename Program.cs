using Mongo.Infrastructure.Models;
using Mongo.WebUI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FlightBookingDatabaseSettings>(builder.Configuration.GetSection("BookingDatabase"));
builder.Services.AddSingleton<MongoService>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
