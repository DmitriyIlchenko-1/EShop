using System.ComponentModel.DataAnnotations;

namespace EShop.Web.Models.Identity;

public class PasswordResetModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string ResultMessage { get; set; }
}