using EShop.Core.Platform.Identity.Configuration;

namespace EShop.Web.Models.Identity;

public class ExternalRegistrationModel : RegistrationBaseModel
{
     
}

public class ExternalRegistrationModelValidator : RegistrationBaseModelValidator<ExternalRegistrationModel>
{
    public ExternalRegistrationModelValidator(UserSettings userSettings) : base(userSettings)
    {
        
        
        
    }
}