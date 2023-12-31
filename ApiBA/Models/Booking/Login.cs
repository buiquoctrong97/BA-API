using System;
namespace ApiBA.Models.Booking
{
	public class Login
	{
		public string email { get; set; }
		public string password { get; set; }
		public string iata_code { get; set; }
        //public string client_id { get; set; }
        //public string client_secret { get; set; }
        public Login(string email,string password,string iata_code)
		{
			this.email = email;
            this.password = password;
			//this.client_id = client_id;
			//this.client_secret = client_secret;
			this.iata_code = iata_code;

        }
	}
}

