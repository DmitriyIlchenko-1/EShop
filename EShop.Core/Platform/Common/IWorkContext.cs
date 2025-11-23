using EShop.Core.Platform.Identity.Domain;

namespace EShop.Core.Platform.Common;

public interface IWorkContext
{
    User CurrentUser { get; }
}