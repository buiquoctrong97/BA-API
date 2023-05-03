using System;
namespace ApiBA.Data
{
	public class API
	{
		public Guid Id { get; set; }
		public string Url { get; set; }
		public Guid? GroupId { get; set; }
		public string Method { get; set; }
		public API()
		{
		}
	}
}

