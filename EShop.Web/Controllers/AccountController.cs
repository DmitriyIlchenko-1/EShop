using EShop.Core.Data;
using EShop.Core.Data.Orders.Extensions;
using EShop.Core.Platform.Common;
using EShop.Web.Common.Controllers;
using EShop.Web.Models.Checkout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers;

[Authorize(Roles = "Registered")]
public class AccountController : EShopBaseController
{
    private readonly AccountHelper _accountHelper;
    private readonly CheckoutHelper _checkoutHelper;
    private readonly ApplicationDbContext _db;
    private readonly IWorkContext _workContext;

    public AccountController(AccountHelper accountHelper, ApplicationDbContext db, IWorkContext workContext,
        CheckoutHelper checkoutHelper)
    {
        _accountHelper = accountHelper;
        _db = db;
        _workContext = workContext;
        _checkoutHelper = checkoutHelper;
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

    public virtual async Task<IActionResult> AddressList()
    {
        var model = await _accountHelper.PrepareAddressListModelAsync(_workContext.CurrentUser.Addresses);
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteAddress(int id)
    {
        if (id < 1)
        {
        }

        var user = _workContext.CurrentUser;
        var address = user.Addresses.FirstOrDefault(x => x.Id == id);
        if (address == null)
        {
        }

        user.Addresses.Remove(address);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(AddressList));
    }
}