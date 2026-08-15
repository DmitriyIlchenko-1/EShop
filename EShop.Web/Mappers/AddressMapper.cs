 

using EShop.Core.Common.Domain;
using EShop.Web.Models.Checkout;
using Riok.Mapperly.Abstractions;

namespace EShop.Web.Mappers;

[Mapper]
public  static partial class AddressMapper
{
    public static partial void ToAddress(this AddressModel from,  Address to);
    
    public static partial void ToAddressModel(this Address from,  AddressModel to);
  
}

 