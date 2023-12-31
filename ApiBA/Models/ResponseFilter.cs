using System;
namespace ApiBA.Models
{
	public class ResponseFilter
	{
		public ResponseFilter()
		{
		}

		public string? ActionName { get; set; }
		public int? StatusCode { get; set; }
		public string? UserName { get; set; }
		public string? AirBookingId { get; set; }
		public string? PnrNumber { get; set; }
		public int Skip { get; set; } = 1;
		public int Take { get; set; } = 50;
	}
}

