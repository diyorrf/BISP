using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace back.Models.DTOs
{
    public class RegisterRequest
    {
        [EmailAddress(ErrorMessage = "Email must be valid")]
        [RegularExpression(@".+@gmail\.com$", ErrorMessage = "Email must be a Gmail address")]
        public string Email { get; set; } = null!;
        
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = null!;
    }
}