using System;
using ApiBA.Models;

namespace ApiBA.Options
{
	public class ApiLoginOption
	{
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ApiUrl { get; set; }
        public string IataCode { get; set; }
    }

}

