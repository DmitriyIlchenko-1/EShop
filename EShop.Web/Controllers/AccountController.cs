using EShop.Core.Common.Services;
using EShop.Core.Data;
using EShop.Core.Data.Orders.Extensions;
using EShop.Core.Platform.Common;
using EShop.Web.Common.Controllers;
using EShop.Web.Models.Checkout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers;

[Authorize(Roles = "Registered")]
public class AccountController : EShopBaseController
{
    private readonly AccountHelper _accountHelper;
    private readonly CheckoutHelper _checkoutHelper;
    private readonly ApplicationDbContext _db;
    private readonly IWorkContext _workContext;
    ICityService _cityService;

    public AccountController(AccountHelper accountHelper, ApplicationDbContext db, IWorkContext workContext,
        CheckoutHelper checkoutHelper, ICityService cityService)
    {
        _accountHelper = accountHelper;
        _db = db;
        _workContext = workContext;
        _checkoutHelper = checkoutHelper;
        _cityService = cityService;
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

    public virtual async Task<IActionResult> UpdateAddress(int id)
    {
        if (id < 1)
        {
        }

        var user = _workContext.CurrentUser;
        var address = user.Addresses.FirstOrDefault(x => x.Id == id);
        if (address == null)
        {
        }
        
        //TODO: use a mapper instead.
        AddressModel model = new AddressModel();
        await _checkoutHelper.PrepareAddressModelAsync(model, address);
        var cities = await _cityService.GetAllAsync();
        model.AvailableCities.Add(new SelectListItem()
        {
            Text = "Select city",
            Value = "0",
        });

        foreach (var city in cities)
        {
            model.AvailableCities.Add(new SelectListItem()
            {
                Text = city.Name,
                Value = city.Id.ToString(),
                Selected = model.CityId == city.Id
            });
        }
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
        if (ModelState.IsValid)
        {
            var entity = model.ToEntity();
            var existing = user.Addresses.FirstOrDefault(x => x == entity);
            if (existing != null)
            {
                
            }
            //TODO: do with a mapper.
            address.FirstName = model.FirstName;
            address.LastName = model.LastName;
            address.PhoneNumber = model.PhoneNumber;
            address.AddressLine1 = model.AddressLine1;
            address.AddressLine2 = model.AddressLine2;
            address.ZipCode = model.ZipCode;
            address.CityId = model.CityId;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(AddressList));
        }
        
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