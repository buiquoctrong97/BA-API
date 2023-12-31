using System;
namespace ApiBA.Models.Booking
{
    public class AirAvailabilityTransit
    {
        public AvailabilitySearches[] availability_searches { get; set; }
        public PaxTypesAALT[] pax_types {get;set;}
        public string trip_type { get; set; } //Loại chặng bay (OW: 1 chiều; RT: Khứ hồi; MC: Nhiều chặng)
        public string? point_of_purchase { get; set; } = "VND"; //Loại đồng tiền mua - mặc định VND

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

