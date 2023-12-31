using System;
namespace ApiBA.Models.Booking
{
	public class ResultLogin
	{
		//public string token { get; set; }
		//public long expires { get; set; }
		//public string refresh_token { get; set; }
		public string access_token { get; set; }
		public ExpireAt expire_at { get; set; }

    }

	public class ExpireAt
	{
		public long seconds { get; set; }
		public long nanos { get; set; }
    }
}

