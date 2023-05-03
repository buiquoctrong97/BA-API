using System;
using System.ComponentModel.DataAnnotations;

namespace ApiBA.Models
{
	public class TokenRequestModel
	{
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}

