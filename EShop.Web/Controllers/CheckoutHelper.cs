using EShop.Core.Common.Domain;
using EShop.Core.Common.Services;
using EShop.Core.Data;
using EShop.Core.Data.Cart.Domain;
using EShop.Core.Data.Cart.Services;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Modules;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Utilities;
using EShop.Web.Factories;
using EShop.Web.Models.Checkout;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers;

public partial class CheckoutHelper
{
    private readonly IWorkContext _workContext;
    private readonly ISession _session;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly IPaymentProviderManager _paymentProviderManager;

    public CheckoutHelper(IWorkContext workContext, IHttpContextAccessor contextAccessor,
        IPaymentProviderManager paymentProviderManager, IAddressModelFactory addressModelFactory)
    {
        _workContext = workContext;
        _session = contextAccessor.HttpContext?.Session;
        _paymentProviderManager = paymentProviderManager;
        _addressModelFactory = addressModelFactory;
    }

    public virtual async Task<CheckoutShippingAddressModel> PrepareShippingAddressModelAsync(ShoppingCart cart)
    {
        Guard.NotNull(cart);
        var user = _workContext.CurrentUser;
        var model = new CheckoutShippingAddressModel();
        foreach (var address in user.Addresses.OrderBy(x => x.Id))
        {
            var addressModel = new AddressModel();
            await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);
            model.ExistingAddresses.Add(addressModel);
        }

        model.NewAddress = new AddressModel();
        await _addressModelFactory.PrepareAddressModelAsync(model.NewAddress,
            null,
            true, true);
        return model;
    }


    public virtual CheckoutPaymentMethodModel PreparePaymentModelAsync()
    {
        var model = new CheckoutPaymentMethodModel();
        foreach (var provider in _paymentProviderManager.GetActivePaymentMethods())
        {
            var metadata = provider.Metadata;
            var prModel = new PaymentMethodModel()
            {
                Name = metadata.FriendlyName,
                SystemName = metadata.SystemName
            };
            model.PaymentMethodModels.Add(prModel);
        }

        if (model.PaymentMethodModels.FirstOrDefault(x => x.Selected) == null)
        {
            var first = model.PaymentMethodModels.FirstOrDefault();
            if (first != null)
            {
                first.Selected = true;
            }
        }

        return model;
    }

    public virtual async Task<CheckoutDataSummaryModel> PrepareCheckoutDataSummaryModelAsync()
    {
        var user = _workContext.CurrentUser;
        var model = new CheckoutDataSummaryModel();
        var shippingAddress = user.ShippingAddress;
        await _addressModelFactory.PrepareAddressModelAsync(model.ShippingAddress, shippingAddress);

        if (_session != null)
        {
            var paymentMethodName = _session.GetString("PaymentMethod");
            var paymentMethod = _paymentProviderManager.GetPaymentMethodBySystemName(paymentMethodName)
                ?.Metadata.FriendlyName ?? string.Empty;
            model.PaymentMethodName = paymentMethod;
        }

        return model;
    }
}