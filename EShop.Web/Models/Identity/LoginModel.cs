using System.ComponentModel.DataAnnotations;
using Microsoft.VisualBasic.CompilerServices;

namespace EShop.Web.Models.Identity;

public class LoginModel
{
    [Required(ErrorMessage = "Enter your email address")]
    [EmailAddress(ErrorMessage = "The email address isn't valid")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Enter your password")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    public bool RememberMe { get; set; }
}