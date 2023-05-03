using System;
namespace ApiBA.Data
{
	public class LoginConfig
	{
		public Guid Id { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public string? Token { get; set; }
		public DateTimeOffset? ModifiedDate { get; set; }
		public LoginConfig()
		{
		}
	}
}

