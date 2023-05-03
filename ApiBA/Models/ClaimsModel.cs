using System;
namespace ApiBA.Models
{
	public class ClaimsModel
	{
		public string UserName { get; set; }
		public string ClaimType { get; set; }
		public string[] ClaimValues { get; set; }
	}
}

