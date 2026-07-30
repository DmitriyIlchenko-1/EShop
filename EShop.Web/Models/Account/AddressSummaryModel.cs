using EShop.Web.Common.Models;

namespace EShop.Web.Models.Account;

public class AddressSummaryModel : BaseModel
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public string AddressLine1 { get; set; }
   
    public string AddressLine2 { get; set; }
    public string ZipCode { get; set; }
    
    public string CityName { get; set; }
}