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
    [JsonIgnore]
    public string AddressString { get; set; }
    [JsonIgnore]
    public bool Selected { get; set; }

   

    public ICollection<SelectListItem> AvailableCities { get; set; } = [];
    
    public Address ToEntity()
    {
        return new Address()
        {
            Id = Id,
            CityId = CityId,
            FirstName = FirstName,
            LastName = LastName,
            PhoneNumber = PhoneNumber,
            AddressLine1 = AddressLine1,
            AddressLine2 = AddressLine2,
            ZipCode = ZipCode,
        };
    }
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