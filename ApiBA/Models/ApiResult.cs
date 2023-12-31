using System;
using ApiBA.Models.Booking;

namespace ApiBA.Models
{
	public class ApiResult
	{
		public Dictionary<string,object> data { get; set; }
		public string id { get; set; }
		public object message { get; set; }
		public bool success { get; set; }
	}

    public class APIResponse
    {
        public object data { get; set; }
        public string id { get; set; }
        public object message { get; set; }
        public bool success { get; set; }
    }
}

