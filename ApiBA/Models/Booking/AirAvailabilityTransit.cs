using System;
namespace ApiBA.Models.Booking
{
    public class AirAvailabilityTransit
    {
        public AvailabilitySearches[] availability_searches { get; set; }
        public PaxTypesAALT[] pax_types {get;set;}
    }

    public class AvailabilitySearches
    {
        public string origin { get; set; }
        public string destination { get; set; }
        public string flight_date { get; set; }
    }
    public class PaxTypesAALT
    {
        public int count { get; set; }
        public string type { get; set; }
    }
    public class PaxTypes
    {
        public int count { get; set; }
        public string type { get; set; }
        public string? sub_type { get; set; }

    }
}

