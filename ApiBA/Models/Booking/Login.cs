using System;
namespace ApiBA.Models.Booking
{
	public class Login
	{
		public string email { get; set; }
		public string password { get; set; }
		public string client_id { get; set; }
		public string client_secret { get; set; }
        public Login(string email,string password,string client_id, string client_secret)
		{
			this.email = email;
            this.password = password;
            this.client_id = client_id;
            this.client_secret = client_secret;
		}
	}
}

