using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mongo.Infrastructure.Models
{
    public class FlightBookingDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string AirplanesCollectionName { get; set; } = null!;

        public string PassengersCollectionName { get; set; } = null!;
    }
}
