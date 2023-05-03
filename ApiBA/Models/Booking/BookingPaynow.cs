using System;
namespace ApiBA.Models.Booking
{
	public class BookingPaynow
	{
		public int number_of_seats { get; set; }
		public string pnr_number { get; set; }
		public BookerDetail booker_detail { get; set; }
		public GuestPaymentInfo[] guest_payment_info { get; set; }

    }
}

