using System.ComponentModel.DataAnnotations;

namespace EShop.Web.Models.Identity;

public abstract  class RegistrationBaseModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Firstname is required")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Lastname is required")]
    public string LastName { get; set; }

    public string Gender { get; set; }
    
    public DateTime BirthDay { get; set; }
}