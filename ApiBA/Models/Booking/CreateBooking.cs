using System;
namespace ApiBA.Models.Booking
{
	public class CreateBooking
	{
		public int number_of_seats { get; set; }
		public string? point_of_sale { get; set; }
		public string? corporate_id { get; set; }
		public BookerDetail booker_detail { get; set; }
		public PaxTypes[] pax_types { get; set; }
		public ItineraryDetails[] itinerary_details { get; set; }
		public FareInfo[] fare_info { get; set; }
		public GuestDetails[] guest_details { get; set; }
		public TravelDocuments[]? travel_documents { get; set; }
		public GuestPaymentInfo[] guest_payment_info { get; set; }
		public SeatAssignmentDetails[]? seat_assignment_details { get; set; }
		public SsrDetails[]? ssr_details { get; set; }
		public PnrContact pnr_contact { get; set; }
		public GuestLoyaltyInfo[]? guest_loyalty_info { get; set; }
		public bool pnr_on_hold_indicator { get; set; }
		public string? promo_code { get; set; }
    }

	public class TravelDocuments
	{
		public int guest_id { get; set; }
		public string nationality { get; set; }
		public Residence residence { get; set; }

    }

	public class Residence
	{
		public string country { get; set; }

    }

	public class GuestPaymentInfo
	{
		public int guest_id { get; set; }
		public double payment_amount { get; set; }
		public string payment_currency { get; set; }
		public string transaction_time { get; set; }
		public string ip_address { get; set; }
    }
	public class PnrContact
	{
		public string name_prefix { get; set; }
		public string given_name { get; set; }
		public string sur_name { get; set; }
		public bool is_preffered_contact { get; set; }
		public string preffered_language { get; set; }
		public Address address { get; set; }

    }
	public class Address
	{
		public string address_type { get; set; }
		public string country_name { get; set; }
		public string phone_number { get; set; }
		public string phone_number_country_code { get; set; }
		public string cell_number { get; set; }
        public string cell_number_country_code { get; set; }
		public bool send_itinerary_to_sms { get; set; }
		public string email_address { get; set; }
		public bool send_itinerary_to_email { get; set; }
    }
	public class GuestLoyaltyInfo
	{
		public int guest_id { get; set; }
		public int segment_id { get; set; }
		public string loyalty_number { get; set; }
    }
}

