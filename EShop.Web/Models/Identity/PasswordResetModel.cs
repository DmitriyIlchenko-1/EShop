using System.ComponentModel.DataAnnotations;

namespace EShop.Web.Models.Identity;

public class PasswordResetModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }
}