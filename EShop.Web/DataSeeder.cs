using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Common.Domain;
using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Catalog.Configuration;
using EShop.Core.Catalog.Products.Price;
using EShop.Core.Content.Media.Domain;
using EShop.Core.Data;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Configuration.Domain;
using EShop.Core.Platform.Routing.Domain;
using EShop.Infrastructure.Utilities;
using EShop.Web.Models.Identity;

public class DataSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IProductAttributeMaterializer _materializer;

    public DataSeeder(ApplicationDbContext dbContext, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedDataAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.MigrateAsync();
        await _dbContext.Database.EnsureCreatedAsync();

        await SeedLabels();
        await SeedUsersAsync();
        await SeedBrandsAsync();
        await SeedDiscountsAsync();
        await SeedProductsAsync();
        await SeedProductReviewsAsync();
        await SeedDeliveriesAsync();
        await SeedProductCategoriesAsync();
        await SeedSlugsAsync();

        //Must be after product and attributes are created
        await SeedSpecificationsAsync();
        await SeedAttributesAsync();
        await SeedCombinationsAsync();

        await SeedSettingsAsync();
    }

    private async Task SeedSlugsAsync()
    {
        var products = await _dbContext.Products.ToListAsync();
        var brands = await _dbContext.Brands.ToListAsync();
        var categories = await _dbContext.Categories.ToListAsync();
        List<UrlRecord> urlRecords = new List<UrlRecord>();
        foreach (var product in products)
        {
            urlRecords.Add(new UrlRecord
            {
                EntityId = product.Id,
                EntityName = product.GetEntityName(),
                Slug = $"FriendlySlugFor-{product.GetEntityName()}-{product.Id}",
                IsActive = true
            });
        }

        foreach (var brand in brands)
        {
            urlRecords.Add(new UrlRecord
            {
                EntityId = brand.Id,
                EntityName = brand.GetEntityName(),
                Slug = $"FriendlySlugFor-{brand.GetEntityName()}-{brand.Id}",
                IsActive = true
            });
        }

        foreach (var category in categories)
        {
            urlRecords.Add(new UrlRecord
            {
                EntityId = category.Id,
                EntityName = category.GetEntityName(),
                Slug = $"FriendlySlugFor-{category.GetEntityName()}-{category.Id}",
                IsActive = true
            });
        }

        await _dbContext.UrlRecords.AddRangeAsync(urlRecords);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedDiscountsAsync()
    {
        var discounts = new List<Discount>();
        for (var i = 0; i < 10; i++)
        {
            var d = new Discount()
            {
                Name = $"Discount {i}",
                DiscountType = i < 5 ? DiscountType.CategoryDiscount : DiscountType.ProductDiscount,
                UsePercentage = i % 2 == 0 ? true : false,
                StartsOnUtc = DateTime.UtcNow.AddMonths(-1),
                EndsOnUtc = DateTime.UtcNow.AddMonths(1),
            };
            var amount = d.UsePercentage
                ? ((decimal)Random.Shared.NextSingle()) * 100
                : (decimal)Random.Shared.Next(10, 300);
            d.DiscountAmount = decimal.Round(amount, 4);
            discounts.Add(d);
        }

        await _dbContext.Discounts.AddRangeAsync(discounts);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedLabels()
    {
        var labels = new List<Label>()
        {
            new Label()
            {
                Name = SystemLabelNames.Sale,
                IconName = "discount-svgrepo-com.svg",
                Template = SystemLabelNames.SaleTemplate,
            },
            new Label()
            {
                Name = "Refurbished",
                IconName = "recycling-svgrepo-com.svg",
                Template = "Refurbished",
            }
        };

        await _dbContext.Labels.AddRangeAsync(labels);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedSettingsAsync()
    {
        var performanceSettings = new Setting()
        {
            Name = "PerformanceSettings.MaxUnavailableCombinations",
            Value = "1"
        };

        await _dbContext.Settings.AddAsync(performanceSettings);


        var catalogSetting = new Setting()
        {
            Name = "CatalogSettings.ShowDescriptionProductList",
            Value = "true"
        };
        var catalogSetting2 = new Setting()
        {
            Name = "CatalogSettings.ShowColorAttributesInLists",
            Value = "true"
        };
        var catalogSetting3 = new Setting()
        {
            Name = "CatalogSettings.ShowReviewsProductList",
            Value = "true"
        };
        
     

        await _dbContext.Settings.AddRangeAsync([catalogSetting, catalogSetting2, catalogSetting3]);
        
        var userSetting = new Setting()
        {
            Name = "UserSettings.FirstNameRequired",
            Value = "true"
        };
        var userSetting2 = new Setting()
        {
            Name = "UserSettings.LastNameRequired",
            Value = "true"
        };
        var userSetting3 = new Setting()
        {
            Name = "UserSettings.BirthdayRequired",
            Value = "true"
        };
        var userSetting4 = new Setting()
        {
            Name = "UserSettings.UserLoginType",
            Value = "UsernameOrEmail"
        };
        
         
        await _dbContext.Settings.AddRangeAsync([userSetting, userSetting2, userSetting3]);

        
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedUsersAsync()
    {
        if (!_dbContext.Users.Any())
        {
            var users = new List<User>
            {
                new User
                {
                    UserName = "testuser1@example.com",
                    Email = "testuser1@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    CreatedOnUtc = DateTime.UtcNow,
                    Active = true,
                    EmailConfirmed = true,
                },
                new User
                {
                    UserName = "testuser2@example.com",
                    Email = "testuser2@example.com",
                    FirstName = "Jane",
                    LastName = "Smith",
                    CreatedOnUtc = DateTime.UtcNow,
                    Active = true,
                    EmailConfirmed = true,
                }
            };

            // Create roles if they don't exist
            var roleManager = _roleManager;

            var roles = new List<Role>
            {
                new Role { Name = UserRoleNameConstants.Guest },
                new Role { Name = UserRoleNameConstants.Registered }
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Name))
                {
                    await roleManager.CreateAsync(role);
                }
            }

            // //Assign users to roles
            // var userRoles = new List<UserRole>();
            // var customerRole = _dbContext.Roles.FirstOrDefault(r => r.Name == "Customer");
            // var adminRole = _dbContext.Roles.FirstOrDefault(r => r.Name == "Administrator");


            _dbContext.Users.AddRange(users);

            await _dbContext.SaveChangesAsync();

            // if (customerRole != null)
            // {
            //     userRoles.Add(new UserRole { UserId = users[0].Id, RoleId = customerRole.Id }); // John is customer
            //     userRoles.Add(new UserRole { UserId = users[1].Id, RoleId = customerRole.Id }); // Jane is customer
            // }
            //
            // if (adminRole != null)
            // {
            //     userRoles.Add(new UserRole { UserId = users[0].Id, RoleId = adminRole.Id }); //John is also admin
            // }
            //
            // _dbContext.UserRoles.AddRange(userRoles);


            await _dbContext.SaveChangesAsync();
        }
    }


    private async Task SeedBrandsAsync()
    {
        if (!_dbContext.Brands.Any())
        {
            var brands = new List<Brand>
            {
                new Brand
                {
                    Name = "Acme Corp", Description = "Manufacturer of widgets", IsPublished = true,
                    CreatedOnUtc = DateTime.UtcNow, ModifiedOnUtc = DateTime.UtcNow, DisplayOrder = 1
                },
                new Brand
                {
                    Name = "Beta Industries", Description = "Provider of innovative solutions", IsPublished = true,
                    CreatedOnUtc = DateTime.UtcNow, ModifiedOnUtc = DateTime.UtcNow, DisplayOrder = 2
                },
                new Brand
                {
                    Name = "Gamma Tech", Description = "Leading technology developer", IsPublished = true,
                    CreatedOnUtc = DateTime.UtcNow, ModifiedOnUtc = DateTime.UtcNow, DisplayOrder = 3
                },
                new Brand
                {
                    Name = "Delta Products", Description = "Quality products for everyone", IsPublished = true,
                    CreatedOnUtc = DateTime.UtcNow, ModifiedOnUtc = DateTime.UtcNow, DisplayOrder = 4
                },
                new Brand
                {
                    Name = "Epsilon Brands", Description = "Stylish and modern designs", IsPublished = true,
                    CreatedOnUtc = DateTime.UtcNow, ModifiedOnUtc = DateTime.UtcNow, DisplayOrder = 5
                },
                new Brand
                {
                    Name = "Zeta Corp", Description = "Widgets provider", IsPublished = true,
                    CreatedOnUtc = DateTime.UtcNow, ModifiedOnUtc = DateTime.UtcNow, DisplayOrder = 1
                },
                new Brand
                {
                    Name = "Alpha Industries", Description = "Innovative solutions provider", IsPublished = true,
                    CreatedOnUtc = DateTime.UtcNow, ModifiedOnUtc = DateTime.UtcNow, DisplayOrder = 2
                },
                new Brand
                {
                    Name = "Omega Tech", Description = "Top technology developer", IsPublished = true,
                    CreatedOnUtc = DateTime.UtcNow, ModifiedOnUtc = DateTime.UtcNow, DisplayOrder = 3
                },
                new Brand
                {
                    Name = "Sigma Products", Description = "High quality products", IsPublished = true,
                    CreatedOnUtc = DateTime.UtcNow, ModifiedOnUtc = DateTime.UtcNow, DisplayOrder = 4
                },
                new Brand
                {
                    Name = "Eeta Brands", Description = "Trendy designs", IsPublished = true,
                    CreatedOnUtc = DateTime.UtcNow, ModifiedOnUtc = DateTime.UtcNow, DisplayOrder = 5
                }
            };

            await _dbContext.Brands.AddRangeAsync(brands);
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task SeedProductsAsync()
    {
        if (!_dbContext.Products.Any())
        {
            var brands = _dbContext.Brands.ToList();
            var products = new List<Product>();

            for (int i = 1; i <= 100; i++)
            {
                var productName = $"Product {i}";
                var description = $"This is a description for {productName}.  It is a high-quality product.";
                var shortDescription = $"Short description for {productName}.";
                var brand = brands.ElementAt(i % brands.Count); // Random brand assignment
                var approvedReviewCount = Random.Shared.Next(0, 20);
                var approvedRatingSum = approvedReviewCount == 0 ? 0 : Random.Shared.Next(approvedReviewCount, approvedReviewCount * 5);
                products.Add(new Product
                {
                    Name = productName,
                    Description = description,
                    ShortDescription = shortDescription,
                    Sku = $"SKU-{i.ToString().PadLeft(3, '0')}", // Create a SKU
                    IsAvailable = true,
                    Published = true,
                    ShowOnHomePage = true,
                    AttributeCombinationRequired = true,
                    DisplayStockQuantity = true,
                    ApprovedReviewCount = approvedReviewCount,
                    ApprovedRatingSum = approvedRatingSum,
                    Price = i == 1
                        ? 0
                        : decimal.Round(
                            decimal.Parse((new Random().Next(10, 1000) + new Random().NextDouble()).ToString()),
                            4),
                    Weight = (decimal)new Random().NextDouble() * 10,
                    Height = (decimal)new Random().NextDouble() * 5,
                    Width = (decimal)new Random().NextDouble() * 5,
                    Length = (decimal)new Random().NextDouble() * 5,
                    StockQuantity = new Random().Next(0, 50),
                    BrandId = brand.Id,
                    CreatedOnUtc = DateTime.UtcNow,
                    ModifiedOnUtc = DateTime.UtcNow,
                    IsShippingEnabled = true, // Enable shipping
                    QuantityUnitId = 1,
                    Gtin = $"GTIN-{i.ToString().PadLeft(3, '0')}" // Generate Gtin
                });
            }

            //Image mapping
            string[] images =
                ("pexels-supliful-14029288.jpg, pexels-makrufinmuhammad-33538457.jpg, pexels-rubaitulazad-17220082.jpg, " +
                 "pexels-afterave-essentials-2011504051-29107590.jpg, pexels-karola-g-4735904.jpg, pexels-dogu-tuncer-339534179-16749129.jpg, " +
                 "pexels-shvetsa-5953781.jpg, pexels-sales-trust-162265874-10825665.jpg, pexels-haipham07-13549321.jpg, pexels-cup-of-couple-8015487.jpg, " +
                 "pexels-karola-g-5632335.jpg, pexels-deise-elen-2149983761-31406906.jpg, pexels-karola-g-4735905.jpg, pexels-ogutomacedo-13013761.jpg, " +
                 "pexels-azka-nandya-91944639-9507137.jpg, pexels-roman-odintsov-7691161.jpg, pexels-cup-of-couple-8015482.jpg, pexels-alesiakozik-7796593.jpg, " +
                 "pexels-roman-odintsov-7691117.jpg, pexels-karola-g-4202325.jpg")
                .Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);

            var imageModels = new List<MediaFile>();
            foreach (var imageName in images)
            {
                var mediaFile = new MediaFile()
                {
                    FileName = imageName,
                    MediaType = ".jpg"
                };
                imageModels.Add(mediaFile);
            }

            _dbContext.MediaFiles.AddRange(imageModels);
            await _dbContext.SaveChangesAsync();

            for (int i = 0; i < products.Count; i++)
            {
                var map = new ProductMedia
                {
                    Product = products[i],
                    MediaFile = imageModels[Random.Shared.Next(0, imageModels.Count)],
                };
                _dbContext.ProductMedias.Add(map);
            }


            //LABELS
            var refurbished = await _dbContext.Labels.SingleOrDefaultAsync(x => x.Name == "Refurbished");
            foreach (var product in products)
            {
                product.Labels.Add(refurbished);
            }


            //DISCOUNTS
            var discounts = await _dbContext
                .Discounts.Where(x => x.DiscountType == DiscountType.ProductDiscount)
                .ToListAsync();
            for (int i = 0; i < products.Count; i++)
            {
                var numberToApply = Random.Shared.Next(0, 3);
                var selectedDiscounts = discounts
                    .OrderBy((x) => Guid.NewGuid())
                    .Take(numberToApply)
                    .ToList();

                foreach (var discount in selectedDiscounts)
                {
                    products[i]
                        .AppliedDiscounts.Add(discount);
                    products[i].HasDiscountsApplied = true;
                }
            }

            _dbContext.Products.AddRange(products);
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task SeedProductReviewsAsync()
    {
        if (!_dbContext.ProductReviews.Any())
        {
            var products = _dbContext.Products.ToList();
            var users = _dbContext.Users.ToList();

            var productReviews = new List<ProductReview>();

            foreach (var product in products)
            {
                for (int i = 1; i <= 20; i++)
                {
                    var user = users[new Random().Next(0, users.Count)];
                    productReviews.Add(new ProductReview
                    {
                        ProductId = product.Id,
                        UserId = user.Id,
                        Title = $"Review {i} for {product.Name}",
                        CommentText = $"This is review {i} for {product.Name}.  It's a great product!",
                        Rating = new Random().Next(1, 5),
                        ReviewerName = user.FirstName + " " + user.LastName,
                        CreatedOnUtc = DateTime.UtcNow,
                        ModifiedOnUtc = DateTime.UtcNow,
                        ReviewStatus = ReviewStatus.Approved
                    });
                }
            }

            _dbContext.ProductReviews.AddRange(productReviews);
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task SeedDeliveriesAsync()
    {
        if (!_dbContext.DeliveryTimes.Any())
        {
            var deliveries = new List<DeliveryTime>
            {
                new DeliveryTime
                {
                    Name = "Standard Delivery", ColorHexValue = "#00FF00", DisplayLocate = "In Stock", DisplayOrder = 1,
                    MinDays = 3, MaxDays = 7, IsDefault = true
                },
                new DeliveryTime
                {
                    Name = "Express Delivery", ColorHexValue = "#FF0000", DisplayLocate = "24 Hours", DisplayOrder = 2,
                    MinDays = 1, MaxDays = 2
                },
                new DeliveryTime
                {
                    Name = "Next Day Delivery", ColorHexValue = "#0000FF", DisplayLocate = "Next Day", DisplayOrder = 3,
                    MinDays = 1, MaxDays = 1
                }
            };

            await _dbContext.DeliveryTimes.AddRangeAsync(deliveries);
            await _dbContext.SaveChangesAsync();

            var products = _dbContext.Products.ToList();
            var deliveryTimes = _dbContext.DeliveryTimes.ToList();

            // Assign deliveries to products
            foreach (var product in products)
            {
                var deliveryTime = deliveryTimes[new Random().Next(0, deliveryTimes.Count)];
                product.DeliveryTimeId = deliveryTime.Id;
            }

            _dbContext.Products.UpdateRange(products);
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task SeedSpecificationsAsync()
    {
        if (!_dbContext.SpecificationAttributes.Any())
        {
            var specificationAttributes = new List<SpecificationAttribute>
            {
                new SpecificationAttribute
                {
                    Name = "Screen Size", Alias = "Screen Size", DisplayOrder = 1,
                },
                new SpecificationAttribute
                {
                    Name = "Resolution", Alias = "Resolution", DisplayOrder = 2,
                },
                new SpecificationAttribute
                    { Name = "RAM", Alias = "RAM", DisplayOrder = 3, },
                new SpecificationAttribute
                {
                    Name = "Storage", Alias = "Storage", DisplayOrder = 4,
                },
                new SpecificationAttribute
                {
                    Name = "Processor", Alias = "Processor", DisplayOrder = 5,
                },
                new SpecificationAttribute
                {
                    Name = "Operating System", Alias = "OS", DisplayOrder = 6,
                },
                new SpecificationAttribute
                {
                    Name = "Color", Alias = "Color", DisplayOrder = 7,
                },
                new SpecificationAttribute
                {
                    Name = "Weight", Alias = "Weight", DisplayOrder = 8,
                },
            };

            await _dbContext.SpecificationAttributes.AddRangeAsync(specificationAttributes);
            await _dbContext.SaveChangesAsync();

            var specificationAttributeOptions = new List<SpecificationAttributeOption>();
            foreach (var attribute in specificationAttributes)
            {
                switch (attribute.Name)
                {
                    case "Screen Size":
                        specificationAttributeOptions.AddRange(new List<SpecificationAttributeOption>
                        {
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "13 inch", DisplayOrder = 1 },
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "15 inch", DisplayOrder = 2 },
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "17 inch", DisplayOrder = 3 },
                        });
                        break;
                    case "Resolution":
                        specificationAttributeOptions.AddRange(new List<SpecificationAttributeOption>
                        {
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "1920x1080", DisplayOrder = 1 },
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "2560x1440", DisplayOrder = 2 },
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "3840x2160", DisplayOrder = 3 },
                        });
                        break;
                    case "RAM":
                        specificationAttributeOptions.AddRange(new List<SpecificationAttributeOption>
                        {
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "8GB", DisplayOrder = 1 },
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "16GB", DisplayOrder = 2 },
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "32GB", DisplayOrder = 3 },
                        });
                        break;
                    case "Storage":
                        specificationAttributeOptions.AddRange(new List<SpecificationAttributeOption>
                        {
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "256GB SSD", DisplayOrder = 1 },
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "512GB SSD", DisplayOrder = 2 },
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "1TB SSD", DisplayOrder = 3 },
                        });
                        break;
                    case "Processor":
                        specificationAttributeOptions.AddRange(new List<SpecificationAttributeOption>
                        {
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "Intel Core i5", DisplayOrder = 1 },
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "Intel Core i7", DisplayOrder = 2 },
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "AMD Ryzen 5", DisplayOrder = 3 },
                        });
                        break;
                    case "Operating System":
                        specificationAttributeOptions.AddRange(new List<SpecificationAttributeOption>
                        {
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "Windows 10", DisplayOrder = 1 },
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "Windows 11", DisplayOrder = 2 },
                            new SpecificationAttributeOption
                                { SpecificationAttributeId = attribute.Id, Name = "macOS", DisplayOrder = 3 },
                        });
                        break;
                    case "Color":
                        specificationAttributeOptions.AddRange(new List<SpecificationAttributeOption>
                        {
                            new SpecificationAttributeOption
                            {
                                SpecificationAttributeId = attribute.Id, Name = "Black", DisplayOrder = 1,
                                Color = "#000000"
                            },
                            new SpecificationAttributeOption
                            {
                                SpecificationAttributeId = attribute.Id, Name = "Silver", DisplayOrder = 2,
                                Color = "#C0C0C0"
                            },
                            new SpecificationAttributeOption
                            {
                                SpecificationAttributeId = attribute.Id, Name = "Blue", DisplayOrder = 3,
                                Color = "#0000FF"
                            },
                        });
                        break;
                    case "Weight":
                        specificationAttributeOptions.AddRange(new List<SpecificationAttributeOption>
                        {
                            new SpecificationAttributeOption
                            {
                                SpecificationAttributeId = attribute.Id, Name = "1 kg", DisplayOrder = 1,
                                NumberValue = 1
                            },
                            new SpecificationAttributeOption
                            {
                                SpecificationAttributeId = attribute.Id, Name = "2 kg", DisplayOrder = 2,
                                NumberValue = 2
                            },
                            new SpecificationAttributeOption
                            {
                                SpecificationAttributeId = attribute.Id, Name = "3 kg", DisplayOrder = 3,
                                NumberValue = 3
                            },
                        });
                        break;
                    default:
                        break;
                }
            }

            _dbContext.SpecificationAttributeOptions.AddRange(specificationAttributeOptions);
            await _dbContext.SaveChangesAsync();

            var products = _dbContext.Products.ToList();
            var specificationAttributeOptionsList = _dbContext.SpecificationAttributeOptions.ToList();

            // Assign specifications to products
            foreach (var product in products)
            {
                var numSpecs = new Random().Next(2, 4); // Randomly assign between 2 and 4 specifications
                var selectedOptions = specificationAttributeOptionsList
                    .OrderBy(x => Guid.NewGuid())
                    .Take(numSpecs)
                    .ToList(); // Get unique option
                foreach (var option in selectedOptions)
                {
                    _dbContext.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
                    {
                        ProductId = product.Id,
                        SpecificationAttributeOptionId = option.Id,
                        DisplayOrder = new Random().Next(1, 4)
                    });
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task SeedAttributesAsync()
    {
        if (!_dbContext.ProductAttributes.Any())
        {
            var attributes = new List<ProductAttribute>
            {
                new ProductAttribute
                {
                    Name = "Color", Alias = "Color", Description = "Product color",
                    DisplayOrder = 1, TextPrompt = "Select color"
                },
                new ProductAttribute
                {
                    Name = "Size", Alias = "Size", Description = "Product size", DisplayOrder = 2,
                    TextPrompt = "Select size"
                },
                new ProductAttribute
                {
                    Name = "Material", Alias = "Material", Description = "Product material",
                    DisplayOrder = 3, TextPrompt = "Select material"
                },
                new ProductAttribute
                {
                    Name = "Capacity", Alias = "Capacity", Description = "Product capacity",
                    DisplayOrder = 4, TextPrompt = "Select capacity"
                },
                new ProductAttribute
                {
                    Name = "Weight", Alias = "Weight", Description = "Product weight",
                    DisplayOrder = 5, TextPrompt = "Select weight"
                },
            };
            await _dbContext.ProductAttributes.AddRangeAsync(attributes);
            await _dbContext.SaveChangesAsync();

            // Create Attribute Options Sets
            var attributeOptionsSets = new List<ProductAttributeOptionsSet>
            {
                new ProductAttributeOptionsSet
                {
                    Name = "Color Options", ProductAttributeId = attributes.First(a => a.Name == "Color")
                        .Id
                },
                new ProductAttributeOptionsSet
                {
                    Name = "Size Options", ProductAttributeId = attributes.First(a => a.Name == "Size")
                        .Id
                },
                new ProductAttributeOptionsSet
                {
                    Name = "Material Options", ProductAttributeId = attributes.First(a => a.Name == "Material")
                        .Id
                },
                new ProductAttributeOptionsSet
                {
                    Name = "Capacity Options", ProductAttributeId = attributes.First(a => a.Name == "Capacity")
                        .Id
                },
                new ProductAttributeOptionsSet
                {
                    Name = "Weight Options", ProductAttributeId = attributes.First(a => a.Name == "Weight")
                        .Id
                },
            };
            await _dbContext.ProductAttributeOptionsSets.AddRangeAsync(attributeOptionsSets);
            await _dbContext.SaveChangesAsync();

            var attributeOptions = new List<ProductAttributeOption>
            {
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Color Options")
                        .Id,
                    Name = "Red", Alias = "Red",
                    Color = "linear-gradient(to bottom right, #ff6db7, #ff6db7 50%, #75e6ff 50%, #75e6ff)",
                    PriceAdjustment = 10, WeightAdjustment = 1,
                    DisplayOrder = 1
                },
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Color Options")
                        .Id,
                    Name = "Blue", Alias = "Blue",
                    Color = "linear-gradient(to bottom right, #ff6db7, #ff6db7 50%, #75e6ff 50%, #75e6ff)",
                    PriceAdjustment = 0, WeightAdjustment = 0,
                    DisplayOrder = 2
                },
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Color Options")
                        .Id,
                    Name = "Green", Alias = "Green",
                    Color = "linear-gradient(to bottom right, black, blue 50%, red 50%, pink)", PriceAdjustment = 5,
                    WeightAdjustment = 0.5m,
                    DisplayOrder = 3
                },
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Size Options")
                        .Id,
                    Name = "Small", Alias = "Small", PriceAdjustment = 0, WeightAdjustment = 0, DisplayOrder = 1
                },
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Size Options")
                        .Id,
                    Name = "Medium", Alias = "Medium", PriceAdjustment = 5, WeightAdjustment = 0.5m, DisplayOrder = 2
                },
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Size Options")
                        .Id,
                    Name = "Large", Alias = "Large", PriceAdjustment = 10, WeightAdjustment = 1, DisplayOrder = 3
                },
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Material Options")
                        .Id,
                    Name = "Cotton", Alias = "Cotton", PriceAdjustment = 0, WeightAdjustment = 0, DisplayOrder = 1
                },
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Material Options")
                        .Id,
                    Name = "Wool", Alias = "Wool", PriceAdjustment = 10, WeightAdjustment = 1, DisplayOrder = 2
                },
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Material Options")
                        .Id,
                    Name = "Polyester", Alias = "Polyester", PriceAdjustment = 5, WeightAdjustment = 0.5m,
                    DisplayOrder = 3
                },
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Capacity Options")
                        .Id,
                    Name = "100ml", Alias = "100ml", PriceAdjustment = 0, WeightAdjustment = 0, DisplayOrder = 1
                },
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Capacity Options")
                        .Id,
                    Name = "200ml", Alias = "200ml", PriceAdjustment = 5, WeightAdjustment = 0.5m, DisplayOrder = 2
                },
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Capacity Options")
                        .Id,
                    Name = "300ml", Alias = "300ml", PriceAdjustment = 10, WeightAdjustment = 1, DisplayOrder = 3
                },
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Weight Options")
                        .Id,
                    Name = "1 kg", Alias = "1kg", PriceAdjustment = 0, WeightAdjustment = 0, DisplayOrder = 1
                },
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Weight Options")
                        .Id,
                    Name = "2 kg", Alias = "2kg", PriceAdjustment = 5, WeightAdjustment = 0.5m, DisplayOrder = 2
                },
                new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = attributeOptionsSets.First(aos => aos.Name == "Weight Options")
                        .Id,
                    Name = "3 kg", Alias = "3kg", PriceAdjustment = 10, WeightAdjustment = 1, DisplayOrder = 3
                },
            };

            await _dbContext.ProductAttributeOptions.AddRangeAsync(attributeOptions);
            await _dbContext.SaveChangesAsync();

            var products = _dbContext.Products.ToList();
            var productAttributes = _dbContext.ProductAttributes.ToList();

            // Assign attributes to products (including Color)
            foreach (var product in products)
            {
                var numAttributes = new Random().Next(3, 6); // Randomly assign between 3 and 5 attributes
                var selectedAttributes = productAttributes
                    .OrderBy(x => Guid.NewGuid())
                    .Take(numAttributes)
                    .ToList(); // Get unique options
                var colorAttribute = selectedAttributes.FirstOrDefault(a => a.Name == "Color");
                if (colorAttribute == null)
                {
                    var color = productAttributes.First(a => a.Name == "Color");
                    selectedAttributes.Add(color);
                }

                foreach (var attribute in selectedAttributes)
                {
                    _dbContext.ProductVariantAttributes.Add(new ProductVariantAttribute
                    {
                        ProductId = product.Id,
                        ProductAttributeId = attribute.Id,
                        DisplayOrder = new Random().Next(1, 4),
                        IsRequired = attribute.Name == "Color", // Color is always required,
                        IsActive = true,
                        AttributeControlType = attribute.Name == "Color"
                            ? AttributeControlType.Swatch
                            : AttributeControlType.RadioButtonPills, //Randomly choose the control type
                    });
                }
            }

            await _dbContext.SaveChangesAsync();

            //Set options on the assigned attributes
            var productVariantAttributes = _dbContext
                .ProductVariantAttributes.Include(x => x.ProductAttribute)
                .ToList();
            var productVariantAttributeOptionsSets = _dbContext
                .ProductAttributeOptionsSets.Include(x => x.ProductAttribute)
                .Include(x => x.ProductAttributeOptions)
                .ToList();
            foreach (var productVariantAttribute in productVariantAttributes)
            {
                var attributeOptionsSet = productVariantAttributeOptionsSets.FirstOrDefault(x =>
                    x.ProductAttributeId == productVariantAttribute.ProductAttributeId);
                if (attributeOptionsSet != null && attributeOptionsSet.ProductAttributeOptions != null)
                {
                    bool preselected = true;
                    foreach (var option in attributeOptionsSet.ProductAttributeOptions)
                    {
                        _dbContext.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
                        {
                            ProductVariantAttributeId = productVariantAttribute.Id,
                            Name = option.Name,
                            Alias = option.Alias,
                            Color = option.Color,
                            IsEssential = true,
                            PriceAdjustment = option.PriceAdjustment,
                            WeightAdjustment = option.WeightAdjustment,
                            DisplayOrder = option.DisplayOrder,
                            IsPreSelected = preselected, // Set as preselected randomly in combinations
                            Quantity = 1
                        });
                        preselected = false;
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task SeedCombinationsAsync()
    {
        var products = _dbContext.Products.ToList();
        var productVariantAttributes = _dbContext
            .ProductVariantAttributes.Include(x => x.ProductAttribute)
            .Include(x => x.ProductVariantAttributeValues)
            .ToList();

        int count = 0;
        foreach (var product in products)
        {
            count++;
            var productVariantAttributeList = productVariantAttributes
                .Where(x => x.ProductId == product.Id)
                .ToList();
            if (!productVariantAttributeList.Any()) continue; // Skip if no attributes

            var combination = new ProductVariantAttributeCombination
            {
                ProductId = product.Id,
                IsActive = false,
                Price = product.Price, // Start with product's base price
                Weight = product.Weight, // Start with product's base weight
                StockQuantity = product.StockQuantity,
            };

            decimal priceAdjustment = 0;
            decimal weightAdjustment = 0;
            var rawAttributes = new List<string>(); // Used to create raw attributes JSON
            var selection = new ProductVariantAttributeSelection();
            foreach (var variantAttribute in productVariantAttributeList)
            {
                var values = variantAttribute.ProductVariantAttributeValues.ToList();
                var randomValue = values.FirstOrDefault(x => x.IsPreSelected);

                Guard.NotNull(randomValue);
                priceAdjustment += randomValue.PriceAdjustment;
                weightAdjustment += randomValue.WeightAdjustment;
                combination.Price += randomValue.PriceAdjustment;
                combination.Weight += randomValue.WeightAdjustment;
                rawAttributes.Add($"{variantAttribute.ProductAttribute.Name}:{randomValue.Name}");
                selection.AddAttribute(variantAttribute.Id, randomValue.Id);
                // Optionally, set IsPreSelected for the value in the combination
            }

            //1 ,24   
// product 1: attributeId: [109,110,111, 112] : valueidL 325, 328, 331, 334  (selection)
// hashcode: -64235192
            combination.HashCode = selection.GetHashCode();
            combination.RawAttributes = "[{\"Key\":5,\"Value\":[18]},{\"Key\":4,\"Value\":[22]}]";
            _dbContext.ProductVariantAttributeCombinations.Add(combination);
        }

        await _dbContext.SaveChangesAsync();
    }


    private async Task SeedProductCategoriesAsync()
    {
        if (!_dbContext.ProductCategories.Any())
        {
            var products = _dbContext.Products.ToList();
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Electronics", DisplayOrder = 1, Description = "Electronics category", IsPublished = true
                },
                new Category
                    { Name = "Clothing", DisplayOrder = 2, Description = "Clothing category", IsPublished = true },
                new Category { Name = "Books", DisplayOrder = 3, Description = "Books category", IsPublished = true }
            };

            //DISCOUNTS
            var discounts = await _dbContext
                .Discounts.Where(x => x.DiscountType == DiscountType.CategoryDiscount)
                .ToListAsync();

            for (int i = 0; i < categories.Count; i++)
            {
                var numberToApply = Random.Shared.Next(0, 3);
                var selectedDiscounts = discounts
                    .OrderBy((x) => Guid.NewGuid())
                    .Take(numberToApply)
                    .ToList();

                foreach (var discount in selectedDiscounts)
                {
                    categories[i]
                        .AppliedDiscounts.Add(discount);
                    categories[i].HasDiscountsApplied = true;
                }
            }

            await _dbContext.Categories.AddRangeAsync(categories);
            await _dbContext.SaveChangesAsync();

            foreach (var product in products)
            {
                var categoryCount = new Random().Next(1, 3); // Assign 1 or 2 categories per product
                var selectedCategories = categories
                    .OrderBy(x => Guid.NewGuid())
                    .Take(categoryCount)
                    .ToList();

                foreach (var category in selectedCategories)
                {
                    _dbContext.ProductCategories.Add(new ProductCategory
                    {
                        ProductId = product.Id,
                        CategoryId = category.Id,
                        DisplayOrder = new Random().Next(1, 4) // Assign a random display order
                    });
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}

// Mock implementation to avoid errors in RoleManager constructor
public class MockRoleStore : IRoleStore<Role>
{
    public void Dispose()
    {
    }

    public Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken) =>
        Task.FromResult(IdentityResult.Success);

    public Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken) =>
        Task.FromResult(IdentityResult.Success);

    public Task<Role?> FindByIdAsync(string roleId, CancellationToken cancellationToken) =>
        Task.FromResult((Role?)null);

    public Task<Role?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken) =>
        Task.FromResult((Role?)null);

    public Task<string?> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken) =>
        Task.FromResult(role.NormalizedName);

    public Task<string?> GetRoleIdAsync(Role role, CancellationToken cancellationToken) =>
        Task.FromResult(role.Id.ToString());

    public Task<string?> GetRoleNameAsync(Role role, CancellationToken cancellationToken) => Task.FromResult(role.Name);

    public Task SetNormalizedRoleNameAsync(Role role, string? normalizedName, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task SetRoleNameAsync(Role role, string? roleName, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken) =>
        Task.FromResult(IdentityResult.Success);
}