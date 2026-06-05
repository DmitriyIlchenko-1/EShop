using System.ComponentModel.DataAnnotations;
using EShop.Core.Catalog.Products.Domain;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Common.Domain;

public class Label : BaseEntity
{
    [MaxLength(15)]
    public string Name { get; set; }

    public string Template { get; set; }
    public string Color { get; set; }
    public string IconName { get; set; }
}


public class SystemLabelNames
{
    public const string Sale = "Sale";
    public const string Recycling = "Recycling";
    public const string NewArrival = "New arrival";
    public const string SoldOut = "Sold out";
    public const string PreOrder = "Pre order";
    public const string NewArrivalTemplate = "New";
    public const string SaleTemplate = "Saving of {0}%!";
}
 
public class ProductLabel
{
    public Product Product { get; set; }
    public Label Label { get; set; }
    public int ProductId { get; set; }
    public int LabelId { get; set; }
    public int Order { get; set; }
}


 