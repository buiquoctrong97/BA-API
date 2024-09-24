using System;
namespace ApiBA.Data
{
	public class TokenWeb
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string? Token { get; set; }
		public DateTimeOffset LastModified { get; set; }
		public TokenWeb()
		{
		}
	}
}

