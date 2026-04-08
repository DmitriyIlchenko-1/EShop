using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Common.Domain;
using EShop.Core.Platform.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

public partial class ApplicationDbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<ProductBrand> ProductBrands { get; set; }

    public DbSet<ProductReview> ProductReviews { get; set; }
    public DbSet<ProductLink> ProductLinks { get; set; }
    public DbSet<ProductVariantAttribute> ProductVariantAttributes { get; set; }
    public DbSet<ProductVariantAttributeCombination> ProductVariantAttributeCombinations { get; set; }
    public DbSet<ProductVariantAttributeValue> ProductVariantAttributeValues { get; set; }
    public DbSet<ProductSpecificationAttribute> ProductSpecificationAttributes { get; set; }
    public DbSet<SpecificationAttribute> SpecificationAttributes { get; set; }
    public DbSet<SpecificationAttributeOption> SpecificationAttributeOptions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }

 
    
}