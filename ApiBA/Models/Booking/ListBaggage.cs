using System;
namespace ApiBA.Models.Booking
{
	public class ListBaggage
	{
		public int number_of_seats { get; set; }
		public BookerDetail booker_detail { get; set; }
		public PaxTypes[] pax_types { get; set; }
		public ItineraryDetails[] itinerary_details { get; set; }
		public FareInfo[] fare_info { get; set; }
		public string sale_date { get; set; }
    }
}

