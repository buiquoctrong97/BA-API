using System;
namespace ApiBA.Data
{
	public class RequestLogs
	{
        public Guid Id { get; set; }
		public DateTimeOffset CreatedDate { get; set; }
		public string CreatedUser { get; set; }
		public string ApiUrl { get; set; }
		public string? Parameters { get; set; }
		public string? ResponseResult { get; set; }
		public string? PnrNumber { get; set; }
		public string? AirBookingId { get; set; }
		public int? StatusCode { get; set; }
        public RequestLogs()
		{
			
		}
	}
}
