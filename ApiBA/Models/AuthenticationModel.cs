using System;
using System.Security.Claims;

namespace ApiBA.Models
{
	public class AuthenticationModel
	{
        public string Message { get; set; }
        //public bool IsAuthenticated { get; set; }
        //public string UserName { get; set; }
        //public string Email { get; set; }
        //public List<string> Roles { get; set; }
        public string Token { get; set; }
        public long Expires { get; set; }
        //public List<Claim> Claims { get; set; }
    }
}

