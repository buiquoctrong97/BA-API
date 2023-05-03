using System;
namespace ApiBA.Models.Booking
{
	public class ResultLogin
	{
		public string token { get; set; }
		public long expires { get; set; }
		public string refresh_token { get; set; }
	}
}

