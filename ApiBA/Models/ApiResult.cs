using System;
namespace ApiBA.Models
{
	public class ApiResult
	{
		public Dictionary<string,object> data { get; set; }
		public string id { get; set; }
		public string message { get; set; }
		public bool success { get; set; }
	}
}

