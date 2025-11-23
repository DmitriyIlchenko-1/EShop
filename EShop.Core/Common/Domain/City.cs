using System.ComponentModel.DataAnnotations;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Core.Common.Domain;


 


public class City : BaseEntity, IDisplayOrder
{
    [StringLength(150)]
    public string Name { get; set; }
    public bool IsBillingEnabled { get; set; }
    public bool IsShippingEnabled { get; set; }
    public bool IsCityEnabled { get; set; }
    public bool IsZipCodeEnabled { get; set; }
    public int DisplayOrder { get; set; }
    
    public ICollection<District> Districts { get; set; } = [];

     
}