using EShop.Core.Platform.Common;
using EShop.Web.Models.Account;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Web.Components;

public class AccountNavigationViewComponent : ViewComponent
{
    private readonly IWorkContext _workContext;

    public AccountNavigationViewComponent(IWorkContext workContext)
    {
        _workContext = workContext;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = _workContext.CurrentUser;
        var model = new UserInfoModel();

        if (user.FirstName != null && user.LastName != null)
        {
            model.FullnameOrUsername = user.FirstName + " " + user.LastName;
        }
        else
        {
            model.FullnameOrUsername = user.Username;
        }

        model.UserSince = $"Has been a customer since {user.CreatedOnUtc.ToString("MMMM")}";
        
        return View(model);
    }
}