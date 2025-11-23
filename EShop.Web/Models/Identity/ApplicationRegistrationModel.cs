using System.ComponentModel.DataAnnotations;

namespace EShop.Web.Models.Identity;

public class ApplicationRegistrationModel : RegistrationBaseModel
{
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Confirm your password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords don't match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }
}