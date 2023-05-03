using System;
using System.ComponentModel.DataAnnotations;

namespace ApiBA.Models
{
	public class ChangePasswordModel
	{
		[Required]
		public string UserName { get; set; }
        [Required]
        public string CurrentPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
	}
}

