using System;
using System.ComponentModel.DataAnnotations;

namespace ApiBA.Models
{
    public class AddRoleModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
    }
}

