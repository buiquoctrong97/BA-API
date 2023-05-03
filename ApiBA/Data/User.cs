using System;
using Microsoft.AspNetCore.Identity;

namespace ApiBA.Data
{
	public class User : IdentityUser
	{
		public double? MaxPayment { get; set; }
        public int? MaxRequest { get; set; }
        //public Guid Id { get; set; }
        //public string UserName { get; set; }
        //public string Password { get; set; }
        //public string Email { get; set; }
    }
}

