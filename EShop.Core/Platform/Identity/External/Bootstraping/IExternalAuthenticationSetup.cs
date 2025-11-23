using Microsoft.AspNetCore.Authentication;

namespace EShop.Core.Platform.Identity.Bootstraping;

public interface IExternalAuthenticationSetup
{
    void Setup(AuthenticationBuilder builder);
}