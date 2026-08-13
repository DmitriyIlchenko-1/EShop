// using EShop.Core.Common.Domain;
// using EShop.Infrastructure.Utilities;
// using EShop.Web.Models.Account;
// using EShop.Web.Models.Checkout;
//
// namespace EShop.Web.Controllers;
//
// public partial class AccountHelper
// {
//     public virtual async Task<AddressListModel> PrepareAddressListModelAsync(IEnumerable<Address> addresses)
//     {
//         Guard.NotNull(addresses);
//         var model = new AddressListModel();
//         var user = _workContext.CurrentUser;
//         foreach (var address in addresses)
//         {
//             var addressModel = new AddressModel()
//             {
//                 Id = address.Id,
//                 AddressLine1 = address.AddressLine1,
//                 AddressLine2 = address.AddressLine2,
//                 CityName = address.CityId.HasValue ? (await _cityService.GetByIdAsync(address.CityId.Value)).Name 
//                                                      : string.Empty,
//                 FirstName = address.FirstName,
//                 LastName = address.LastName,
//                 PhoneNumber = address.PhoneNumber,
//                 ZipCode = address.ZipCode,
//             };
//             addressModel.IsDefault = user.ShippingAddressId == address.Id;
//             model.Addresses.Add(addressModel);
//         }
//         return model;
//     }
//
//    
// }