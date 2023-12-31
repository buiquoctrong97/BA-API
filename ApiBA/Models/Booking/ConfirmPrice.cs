using System;
namespace ApiBA.Models.Booking
{
	public class ConfirmPrice
	{
		public int number_of_seats { get; set; }
		//public string? point_of_sale { get; set; }
		public BookerDetail booker_detail { get; set; }
		//public PaxTypes pax_types { get; set; }
		public ItineraryDetails[] itinerary_details { get; set; }
		public FareInfoConfirm[] fare_info { get; set; }
		public GuestDetails[] guest_details { get; set; }
		//public SeatAssignmentDetails[]? seat_assignment_details { get; set; }
		//public SsrDetails[]? ssr_details { get; set; }
		//public string? promo_code { get; set; }
		//public string? corporate_id { get; set; }

		public Pnr_Contact pnr_contact { get; set; }

        public string trip_type { get; set; } //Loại chặng bay (OW: 1 chiều; RT: Khứ hồi; MC: Nhiều chặng)
		public string? point_of_purchase { get; set; } = "VND"; //Loại đồng tiền mua - mặc định VND
		public string air_booking_id { get; set; } //air_booking_id (lấy dữ liệu: air-availability#id hoặc confirm-price#id )

    }
	public class FareInfoConfirm {
        public string fare_basis { get; set; }
    }

    public class Pnr_Contact
	{
		public ConfirmAddress address { get; set; }

        public string given_name { get; set; }
        public string sur_name { get; set; }
    }
	public class BookerDetail
	{
		public string given_name { get; set; }
		public string sur_name { get; set; }

    }
	public class ConfirmAddress
	{
		public string email { get; set; }
		public string phone_number { get; set; }

    }
	public class ItineraryDetails
	{
		public int flight_segment_group_id { get; set; } 
		public int segment_id { get; set; }
		public int flight_number { get; set; }
		public string flight_date { get; set; }
		public string? flight_status { get; set; }
		public string board_point { get; set; }
		public string off_point { get; set; }
		//public string cabin_class { get; set; } //remove
		public string booking_class { get; set; } //Hạng ghế của chuyến bay (lấy dữ liệu: air_availability#data#trip_info#booking_class#group_fare#fare_class)

        //public bool is_through_flight { get; set; } //remove
		public string carrier_code { get; set; } //Hãng bay: (lấy dữ liệu: air_availability#data#trip_info#segment_info#flight_info#carrier_code)
        public string fare_basis { get; set; }
		//public string added_time { get; set; } //remove

		//public string scheduled_departure_datetime { get; set; }
		//public string scheduled_arrival_time { get; set; }

		//public int departure_terminal { get; set; }
		//public int arrival_terminal { get; set; }
		


    }
	public class FareInfo
	{
		public string fare_level { get; set; }
		public string fare_basis { get; set; }
		public string fare_type { get; set; }
		public string pax_type { get; set; }
		public double base_fare { get; set; }
		public string currency { get; set; }
		public bool return_restriction_id { get; set; }
		public int fare_transaction_id { get; set; }
		public int segment_id { get; set; }
		public int pricing_unit_id { get; set; }
		public int fare_component_id { get; set; }

    }
	public class GuestDetails
	{
		public string given_name { get; set; }
		public string sur_name { get; set; }
		public string name_prefix { get; set; }
		//public string guest_type { get; set; } //change name to type
		public string type { get; set; } //ADULT
		public string? date_of_birth { get; set; }
		public int? family_id { get; set; }
		public int guest_id { get; set; }
		public int? parent_guest_id { get; set; } //Bắt buộc đối với hành khách là em bé(INFANT)
												  //và giá trị của trường này phải là guest_id của hành khách người lớn(ADULT)
												  //mà em bé(INFANT) được liên kết.

    }
	public class SeatAssignmentDetails
	{
		public string child_board_point { get; set; }
		public string child_off_point { get; set; }
		public int segment_id { get; set; }
        public GuestSeatDetails[] guest_seat_details { get; set; }

    }
	public class GuestSeatDetails
	{
		public int guest_id { get; set; }
		public string seat_numbers { get; set; }
    }
	public class SsrDetails
	{
		public string ssr_code { get; set; }
		public string? ssr_comments { get; set; }
		public int segment_id { get; set; }
		public int guest_id { get; set; }
		public SsrFieldDetailsType[] ssr_field_details_type { get; set; }

    }
	public class SsrFieldDetailsType
	{
		public string ssr_field_name { get; set; }
		public string ssr_field_value { get; set; }
    }

	public class PreConfirmPrice
	{
		public PaxTypes pax_types { get; set; }

        public ItineraryDetails itinerary_details { get; set; }
		//public GuestDetails guest_details { get; set; }
		//public Pnr_Contact pnr_contact { get; set; }
		public string trip_type { get; set; }
		public string? point_of_purchase { get; set; }
		public string air_booking_id { get; set; }
    }
}

