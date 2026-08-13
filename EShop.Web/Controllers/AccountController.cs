using EShop.Core.Common.Services;
using EShop.Core.Data;
using EShop.Core.Data.Orders.Extensions;
using EShop.Core.Platform.Common;
using EShop.Infrastructure.Data;
using EShop.Web.Common.Controllers;
using EShop.Web.Factories;
using EShop.Web.Mappers;
using EShop.Web.Models.Checkout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

namespace EShop.Web.Controllers;

[Authorize(Roles = "Registered")]
public class AccountController : EShopBaseController
{
    private readonly AccountHelper _accountHelper;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly ApplicationDbContext _db;
    private readonly IWorkContext _workContext;


    public AccountController(AccountHelper accountHelper, ApplicationDbContext db, IWorkContext workContext,
        IAddressModelFactory addressModelFactory)
    {
        _accountHelper = accountHelper;
        _db = db;
        _workContext = workContext;
        _addressModelFactory = addressModelFactory;
    }

    public virtual async Task<IActionResult> OrderList()
    {
        var orders = await _db
            .Orders
            .ApplyStandardFilter(_workContext.CurrentUser.Id)
            .ToListAsync();
        var model = await _accountHelper.PrepareOrderListModelAsync(orders);
        return View(model);
    }


    public virtual async Task<IActionResult> OrderDetails(int id)
    {
        var order = await _db
            .Orders.AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.OrderItems)
            .ThenInclude(x => x.Product)
            .Include(x => x.ShippingAddress)
            .FirstOrDefaultAsync(x => x.UserId == _workContext.CurrentUser.Id && x.Id == id);
        
        if (order == null)
        {
            return NotFound();
        }

        var model = await _accountHelper.PrepareOrderDetailModelAsync(order);
        return View("Order", model);
    }

    public virtual async Task<IActionResult> AddressList()
    {
        ICollection<AddressModel> addresses = new List<AddressModel>();
        foreach (var address in _workContext.CurrentUser.Addresses)
        {
            var model = new AddressModel();
            await _addressModelFactory.PrepareAddressModelAsync(model, address);
            addresses.Add(model);
        }

        return View(addresses);
    }

    public virtual async Task<IActionResult> UpdateAddress(int id)
    {
        var user = _workContext.CurrentUser;
        var address = user.Addresses.FirstOrDefault(x => x.Id == id);
        if (address == null)
        {
            return NotFound();
        }

        var model = new AddressModel
        {
            EnableSelectAsDefault = true,
        };
        await _addressModelFactory.PrepareAddressModelAsync(model, address, loadCities: true);
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> UpdateAddress(AddressModel model, int id)
    {
        var user = _workContext.CurrentUser;
        var address = user.Addresses.FirstOrDefault(x => x.Id == id);
        if (address == null)
        {
            return NotFound();
        }

        model.EnableSelectAsDefault = true;

        if (ModelState.IsValid)
        {
            model.ToAddress(address);

            if (model.EnableSelectAsDefault)
            {
                if (model.IsDefault)
                {
                    user.ShippingAddress = address;
                }
                else
                {
                    if (user.ShippingAddressId == address.Id)
                    {
                        user.ShippingAddress = null;
                    }
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(AddressList));
        }

        await _addressModelFactory.PrepareAddressModelAsync(model, address, loadCities: true);
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteAddress(int id)
    {
        var user = _workContext.CurrentUser;
        var address = user.Addresses.FirstOrDefault(x => x.Id == id);
        if (address == null)
        {
            return NotFound();
        }

        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(AddressList));
    }
}