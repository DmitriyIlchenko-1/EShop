using EShop.Core.Data;
using EShop.Core.Data.Orders.Extensions;
using EShop.Core.Platform.Common;
using EShop.Web.Common.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers;

[Authorize(Roles = "Registered")]
public class AccountController : EShopBaseController
{
    private readonly AccountHelper _accountHelper;
    private readonly ApplicationDbContext _db;
    private readonly IWorkContext _workContext;

    public AccountController(AccountHelper accountHelper, ApplicationDbContext db, IWorkContext workContext)
    {
        _accountHelper = accountHelper;
        _db = db;
        _workContext = workContext;
    }

    public virtual async Task<IActionResult> OrderList()
    {
        var model = await _accountHelper.PrepareOrderListModelAsync();
        return View(model);
    }


    public virtual async Task<IActionResult> OrderDetails(int id)
    {
        if (id < 1)
        {
        }

        var order = await _db
            .Orders.AsNoTracking()
            .ApplyStandardFilter(_workContext.CurrentUser.Id)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
        {
        }

        var model = await _accountHelper.PrepareOrderDetailModelAsync(order);
        return View("Order", model);
    }
}