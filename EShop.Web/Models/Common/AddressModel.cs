using System.ComponentModel;
using EShop.Core.Common.Domain;
using EShop.Infrastructure.Domain;
using EShop.Web.Common.Models;
using EShop.Web.Models.Account;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace EShop.Web.Models.Checkout;

public class AddressModel : BaseModel
{
    
    public string Fullname => string.Join(" ", FirstName, LastName);
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public string AddressLine1 { get; set; }
   
    public string AddressLine2 { get; set; }
    public string ZipCode { get; set; }
    public int CityId { get; set; }
    public string CityName { get; set; }
    
    [DisplayName("Select as default shipping address")]
    public bool IsDefault { get; set; }
    public bool EnableSelectAsDefault { get; set; }
    public ICollection<SelectListItem> AvailableCities { get; set; } = [];
}

public class AddressModelValidator : AbstractValidator<AddressModel>
{
    public AddressModelValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty();
        RuleFor(x => x.LastName)
            .NotEmpty();
        RuleFor(x => x.AddressLine1)
            .NotEmpty();
        RuleFor(x => x.ZipCode)
            .NotEmpty();
        RuleFor(x => x.CityId)
            .GreaterThanOrEqualTo(1)
            .WithMessage("City must not be empty");
    }
}