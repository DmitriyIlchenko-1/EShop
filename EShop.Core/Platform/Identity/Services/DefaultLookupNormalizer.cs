using Microsoft.AspNetCore.Identity;

namespace EShop.Core.Platform.Identity.Services;

public class DefaultLookupNormalizer : ILookupNormalizer
{
    public string NormalizeName(string name)
    {
        return name;
    }

    public string NormalizeEmail(string email)
    {
        return email;
    }
}