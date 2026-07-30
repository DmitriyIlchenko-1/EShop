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
using EShop.Core.Data.Cart.Domain;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Configuration.Domain;
using EShop.Core.Platform.Configuration.Services;
using EShop.Core.Platform.Routing.Domain;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Utilities;
using EShop.Web.Models.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json.Linq;

public class DataSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IProductAttributeMaterializer _materializer;
    private readonly ISettingFactory _settingFactory;

    public DataSeeder(ApplicationDbContext dbContext, UserManager<User> userManager, RoleManager<Role> roleManager,
        ISettingFactory settingFactory)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _settingFactory = settingFactory;
    }

    public async Task SeedDataAsync()
    {
        bool needsSeeding = false;
        if (await _dbContext.Database.EnsureCreatedAsync() || (_dbContext.Database.HasPendingModelChanges() ||
                                                               (await _dbContext.Database
                                                                   .GetPendingMigrationsAsync()).Any()))
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.MigrateAsync();
            await _dbContext.Database.EnsureCreatedAsync();
            needsSeeding = true;
        }

        if (!needsSeeding)
        {
            return;
        }


        await SeedCitiesAsync();
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

    private async Task SeedCitiesAsync()
    {
        var cities = await _dbContext.Cities.ToListAsync();
        if (cities.Any())
        {
            return;
        }

        var appDataRoot = EngineContext.Current.ApplicationContext.AppDataRoot;
        var cityFile = appDataRoot.GetFileInfo("InstallationData/cities.txt");
        await using var stream = cityFile.CreateReadStream();
        using var reader = new StreamReader(stream);
        string cityString = await reader.ReadToEndAsync();
        var cityArray = cityString.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var cityName in cityArray)
        {
            var city = new City();
            city.Name = cityName;
            cities.Add(city);
        }

        _dbContext.AddRange(cities);
        await _dbContext.SaveChangesAsync();
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
                Name = "Recycling",
                Content = "Recycled",
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

        var inventorySetting = new Setting()
        {
            Name = "InventorySettings.InStockThreshold",
            Value = "20"
        };


        await _dbContext.Settings.AddAsync(inventorySetting);


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
                    //Admin123-
                    Username = "admin123",
                    Email = "cortezz1ty@gmail.com",
                    FirstName = "John",
                    LastName = "Doe",
                    CreatedOnUtc = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true,
                    SecurityStamp = "",
                    ShippingAddressId = 4,
                    Addresses = new List<Address>()
                    {
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                            new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "555-0101",
                            AddressLine1 = "123 Main St",
                            AddressLine2 = "Apt 4B",
                            ZipCode = "90210",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 12
                        },
                        new Address
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "555-0102",
                            AddressLine1 = "456 Oak Avenue",
                            AddressLine2 = null,
                            ZipCode = "33101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 84
                        },
                        new Address
                        {
                            FirstName = "Michael",
                            LastName = "Johnson",
                            PhoneNumber = "555-0103",
                            AddressLine1 = "789 Pine Road",
                            AddressLine2 = "Suite 100",
                            ZipCode = "60601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 33
                        },
                        new Address
                        {
                            FirstName = "Emily",
                            LastName = "Williams",
                            PhoneNumber = "555-0104",
                            AddressLine1 = "321 Maple Drive",
                            AddressLine2 = null,
                            ZipCode = "10001",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 7
                        },
                        new Address
                        {
                            FirstName = "David",
                            LastName = "Brown",
                            PhoneNumber = "555-0105",
                            AddressLine1 = "654 Elm Street",
                            AddressLine2 = "Unit 2",
                            ZipCode = "73301",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 91
                        },
                        new Address
                        {
                            FirstName = "Sarah",
                            LastName = "Jones",
                            PhoneNumber = "555-0106",
                            AddressLine1 = "987 Cedar Lane",
                            AddressLine2 = null,
                            ZipCode = "94105",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 45
                        },
                        new Address
                        {
                            FirstName = "James",
                            LastName = "Garcia",
                            PhoneNumber = "555-0107",
                            AddressLine1 = "147 Birch Blvd",
                            AddressLine2 = "Building C",
                            ZipCode = "98101",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 68
                        },
                        new Address
                        {
                            FirstName = "Jessica",
                            LastName = "Miller",
                            PhoneNumber = "555-0108",
                            AddressLine1 = "258 Walnut Way",
                            AddressLine2 = null,
                            ZipCode = "30301",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 21
                        },
                        new Address
                        {
                            FirstName = "Robert",
                            LastName = "Davis",
                            PhoneNumber = "555-0109",
                            AddressLine1 = "369 Cherry Court",
                            AddressLine2 = "Floor 3",
                            ZipCode = "02108",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 55
                        },
                        new Address
                        {
                            FirstName = "Laura",
                            LastName = "Rodriguez",
                            PhoneNumber = "555-0110",
                            AddressLine1 = "741 Spruce Circle",
                            AddressLine2 = null,
                            ZipCode = "75201",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 89
                        },
                        new Address
                        {
                            FirstName = "William",
                            LastName = "Martinez",
                            PhoneNumber = "555-0111",
                            AddressLine1 = "852 Aspen Trace",
                            AddressLine2 = "Apt 12",
                            ZipCode = "80202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 15
                        },
                        new Address
                        {
                            FirstName = "Linda",
                            LastName = "Hernandez",
                            PhoneNumber = "555-0112",
                            AddressLine1 = "963 Willow Street",
                            AddressLine2 = null,
                            ZipCode = "19102",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 77
                        },
                        new Address
                        {
                            FirstName = "Richard",
                            LastName = "Lopez",
                            PhoneNumber = "555-0113",
                            AddressLine1 = "159 Cypress Avenue",
                            AddressLine2 = "Suite 5B",
                            ZipCode = "48226",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 38
                        },
                        new Address
                        {
                            FirstName = "Barbara",
                            LastName = "Gonzalez",
                            PhoneNumber = "555-0114",
                            AddressLine1 = "753 Redwood Road",
                            AddressLine2 = null,
                            ZipCode = "28202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 94
                        },
                        new Address
                        {
                            FirstName = "Joseph",
                            LastName = "Wilson",
                            PhoneNumber = "555-0115",
                            AddressLine1 = "456 Magnolia Blvd",
                            AddressLine2 = "Unit 90",
                            ZipCode = "37203",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 2
                        },
                        new Address
                        {
                            FirstName = "Susan",
                            LastName = "Anderson",
                            PhoneNumber = "555-0116",
                            AddressLine1 = "123 Sycamore Lane",
                            AddressLine2 = null,
                            ZipCode = "85004",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 49
                        },
                        new Address
                        {
                            FirstName = "Thomas",
                            LastName = "Thomas",
                            PhoneNumber = "555-0117",
                            AddressLine1 = "852 Poplar Drive",
                            AddressLine2 = "Apt 101",
                            ZipCode = "97204",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 62
                        },
                        new Address
                        {
                            FirstName = "Margaret",
                            LastName = "Taylor",
                            PhoneNumber = "555-0118",
                            AddressLine1 = "963 Hickory Court",
                            AddressLine2 = null,
                            ZipCode = "20001",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 8
                        },
                         new Address
                        {
                            FirstName = "Jessica",
                            LastName = "Miller",
                            PhoneNumber = "555-0108",
                            AddressLine1 = "258 Walnut Way",
                            AddressLine2 = null,
                            ZipCode = "30301",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 21
                        },
                        new Address
                        {
                            FirstName = "Robert",
                            LastName = "Davis",
                            PhoneNumber = "555-0109",
                            AddressLine1 = "369 Cherry Court",
                            AddressLine2 = "Floor 3",
                            ZipCode = "02108",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 55
                        },
                        new Address
                        {
                            FirstName = "Laura",
                            LastName = "Rodriguez",
                            PhoneNumber = "555-0110",
                            AddressLine1 = "741 Spruce Circle",
                            AddressLine2 = null,
                            ZipCode = "75201",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 89
                        },
                        new Address
                        {
                            FirstName = "William",
                            LastName = "Martinez",
                            PhoneNumber = "555-0111",
                            AddressLine1 = "852 Aspen Trace",
                            AddressLine2 = "Apt 12",
                            ZipCode = "80202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 15
                        },
                        new Address
                        {
                            FirstName = "Linda",
                            LastName = "Hernandez",
                            PhoneNumber = "555-0112",
                            AddressLine1 = "963 Willow Street",
                            AddressLine2 = null,
                            ZipCode = "19102",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 77
                        },
                        new Address
                        {
                            FirstName = "Richard",
                            LastName = "Lopez",
                            PhoneNumber = "555-0113",
                            AddressLine1 = "159 Cypress Avenue",
                            AddressLine2 = "Suite 5B",
                            ZipCode = "48226",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 38
                        },
                        new Address
                        {
                            FirstName = "Barbara",
                            LastName = "Gonzalez",
                            PhoneNumber = "555-0114",
                            AddressLine1 = "753 Redwood Road",
                            AddressLine2 = null,
                            ZipCode = "28202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 94
                        },
                        new Address
                        {
                            FirstName = "Joseph",
                            LastName = "Wilson",
                            PhoneNumber = "555-0115",
                            AddressLine1 = "456 Magnolia Blvd",
                            AddressLine2 = "Unit 90",
                            ZipCode = "37203",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 2
                        },
                        new Address
                        {
                            FirstName = "Susan",
                            LastName = "Anderson",
                            PhoneNumber = "555-0116",
                            AddressLine1 = "123 Sycamore Lane",
                            AddressLine2 = null,
                            ZipCode = "85004",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 49
                        },
                        new Address
                        {
                            FirstName = "Thomas",
                            LastName = "Thomas",
                            PhoneNumber = "555-0117",
                            AddressLine1 = "852 Poplar Drive",
                            AddressLine2 = "Apt 101",
                            ZipCode = "97204",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 62
                        },
                        new Address
                        {
                            FirstName = "Margaret",
                            LastName = "Taylor",
                            PhoneNumber = "555-0118",
                            AddressLine1 = "963 Hickory Court",
                            AddressLine2 = null,
                            ZipCode = "20001",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 8
                        },
                         new Address
                        {
                            FirstName = "Jessica",
                            LastName = "Miller",
                            PhoneNumber = "555-0108",
                            AddressLine1 = "258 Walnut Way",
                            AddressLine2 = null,
                            ZipCode = "30301",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 21
                        },
                        new Address
                        {
                            FirstName = "Robert",
                            LastName = "Davis",
                            PhoneNumber = "555-0109",
                            AddressLine1 = "369 Cherry Court",
                            AddressLine2 = "Floor 3",
                            ZipCode = "02108",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 55
                        },
                        new Address
                        {
                            FirstName = "Laura",
                            LastName = "Rodriguez",
                            PhoneNumber = "555-0110",
                            AddressLine1 = "741 Spruce Circle",
                            AddressLine2 = null,
                            ZipCode = "75201",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 89
                        },
                        new Address
                        {
                            FirstName = "William",
                            LastName = "Martinez",
                            PhoneNumber = "555-0111",
                            AddressLine1 = "852 Aspen Trace",
                            AddressLine2 = "Apt 12",
                            ZipCode = "80202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 15
                        },
                        new Address
                        {
                            FirstName = "Linda",
                            LastName = "Hernandez",
                            PhoneNumber = "555-0112",
                            AddressLine1 = "963 Willow Street",
                            AddressLine2 = null,
                            ZipCode = "19102",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 77
                        },
                        new Address
                        {
                            FirstName = "Richard",
                            LastName = "Lopez",
                            PhoneNumber = "555-0113",
                            AddressLine1 = "159 Cypress Avenue",
                            AddressLine2 = "Suite 5B",
                            ZipCode = "48226",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 38
                        },
                        new Address
                        {
                            FirstName = "Barbara",
                            LastName = "Gonzalez",
                            PhoneNumber = "555-0114",
                            AddressLine1 = "753 Redwood Road",
                            AddressLine2 = null,
                            ZipCode = "28202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 94
                        },
                        new Address
                        {
                            FirstName = "Joseph",
                            LastName = "Wilson",
                            PhoneNumber = "555-0115",
                            AddressLine1 = "456 Magnolia Blvd",
                            AddressLine2 = "Unit 90",
                            ZipCode = "37203",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 2
                        },
                        new Address
                        {
                            FirstName = "Susan",
                            LastName = "Anderson",
                            PhoneNumber = "555-0116",
                            AddressLine1 = "123 Sycamore Lane",
                            AddressLine2 = null,
                            ZipCode = "85004",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 49
                        },
                        new Address
                        {
                            FirstName = "Thomas",
                            LastName = "Thomas",
                            PhoneNumber = "555-0117",
                            AddressLine1 = "852 Poplar Drive",
                            AddressLine2 = "Apt 101",
                            ZipCode = "97204",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 62
                        },
                        new Address
                        {
                            FirstName = "Margaret",
                            LastName = "Taylor",
                            PhoneNumber = "555-0118",
                            AddressLine1 = "963 Hickory Court",
                            AddressLine2 = null,
                            ZipCode = "20001",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 8
                        },
                         new Address
                        {
                            FirstName = "Jessica",
                            LastName = "Miller",
                            PhoneNumber = "555-0108",
                            AddressLine1 = "258 Walnut Way",
                            AddressLine2 = null,
                            ZipCode = "30301",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 21
                        },
                        new Address
                        {
                            FirstName = "Robert",
                            LastName = "Davis",
                            PhoneNumber = "555-0109",
                            AddressLine1 = "369 Cherry Court",
                            AddressLine2 = "Floor 3",
                            ZipCode = "02108",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 55
                        },
                        new Address
                        {
                            FirstName = "Laura",
                            LastName = "Rodriguez",
                            PhoneNumber = "555-0110",
                            AddressLine1 = "741 Spruce Circle",
                            AddressLine2 = null,
                            ZipCode = "75201",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 89
                        },
                        new Address
                        {
                            FirstName = "William",
                            LastName = "Martinez",
                            PhoneNumber = "555-0111",
                            AddressLine1 = "852 Aspen Trace",
                            AddressLine2 = "Apt 12",
                            ZipCode = "80202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 15
                        },
                        new Address
                        {
                            FirstName = "Linda",
                            LastName = "Hernandez",
                            PhoneNumber = "555-0112",
                            AddressLine1 = "963 Willow Street",
                            AddressLine2 = null,
                            ZipCode = "19102",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 77
                        },
                        new Address
                        {
                            FirstName = "Richard",
                            LastName = "Lopez",
                            PhoneNumber = "555-0113",
                            AddressLine1 = "159 Cypress Avenue",
                            AddressLine2 = "Suite 5B",
                            ZipCode = "48226",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 38
                        },
                        new Address
                        {
                            FirstName = "Barbara",
                            LastName = "Gonzalez",
                            PhoneNumber = "555-0114",
                            AddressLine1 = "753 Redwood Road",
                            AddressLine2 = null,
                            ZipCode = "28202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 94
                        },
                        new Address
                        {
                            FirstName = "Joseph",
                            LastName = "Wilson",
                            PhoneNumber = "555-0115",
                            AddressLine1 = "456 Magnolia Blvd",
                            AddressLine2 = "Unit 90",
                            ZipCode = "37203",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 2
                        },
                        new Address
                        {
                            FirstName = "Susan",
                            LastName = "Anderson",
                            PhoneNumber = "555-0116",
                            AddressLine1 = "123 Sycamore Lane",
                            AddressLine2 = null,
                            ZipCode = "85004",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 49
                        },
                        new Address
                        {
                            FirstName = "Thomas",
                            LastName = "Thomas",
                            PhoneNumber = "555-0117",
                            AddressLine1 = "852 Poplar Drive",
                            AddressLine2 = "Apt 101",
                            ZipCode = "97204",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 62
                        },
                        new Address
                        {
                            FirstName = "Margaret",
                            LastName = "Taylor",
                            PhoneNumber = "555-0118",
                            AddressLine1 = "963 Hickory Court",
                            AddressLine2 = null,
                            ZipCode = "20001",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 8
                        },
                         new Address
                        {
                            FirstName = "Jessica",
                            LastName = "Miller",
                            PhoneNumber = "555-0108",
                            AddressLine1 = "258 Walnut Way",
                            AddressLine2 = null,
                            ZipCode = "30301",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 21
                        },
                        new Address
                        {
                            FirstName = "Robert",
                            LastName = "Davis",
                            PhoneNumber = "555-0109",
                            AddressLine1 = "369 Cherry Court",
                            AddressLine2 = "Floor 3",
                            ZipCode = "02108",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 55
                        },
                        new Address
                        {
                            FirstName = "Laura",
                            LastName = "Rodriguez",
                            PhoneNumber = "555-0110",
                            AddressLine1 = "741 Spruce Circle",
                            AddressLine2 = null,
                            ZipCode = "75201",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 89
                        },
                        new Address
                        {
                            FirstName = "William",
                            LastName = "Martinez",
                            PhoneNumber = "555-0111",
                            AddressLine1 = "852 Aspen Trace",
                            AddressLine2 = "Apt 12",
                            ZipCode = "80202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 15
                        },
                        new Address
                        {
                            FirstName = "Linda",
                            LastName = "Hernandez",
                            PhoneNumber = "555-0112",
                            AddressLine1 = "963 Willow Street",
                            AddressLine2 = null,
                            ZipCode = "19102",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 77
                        },
                        new Address
                        {
                            FirstName = "Richard",
                            LastName = "Lopez",
                            PhoneNumber = "555-0113",
                            AddressLine1 = "159 Cypress Avenue",
                            AddressLine2 = "Suite 5B",
                            ZipCode = "48226",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 38
                        },
                        new Address
                        {
                            FirstName = "Barbara",
                            LastName = "Gonzalez",
                            PhoneNumber = "555-0114",
                            AddressLine1 = "753 Redwood Road",
                            AddressLine2 = null,
                            ZipCode = "28202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 94
                        },
                        new Address
                        {
                            FirstName = "Joseph",
                            LastName = "Wilson",
                            PhoneNumber = "555-0115",
                            AddressLine1 = "456 Magnolia Blvd",
                            AddressLine2 = "Unit 90",
                            ZipCode = "37203",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 2
                        },
                        new Address
                        {
                            FirstName = "Susan",
                            LastName = "Anderson",
                            PhoneNumber = "555-0116",
                            AddressLine1 = "123 Sycamore Lane",
                            AddressLine2 = null,
                            ZipCode = "85004",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 49
                        },
                        new Address
                        {
                            FirstName = "Thomas",
                            LastName = "Thomas",
                            PhoneNumber = "555-0117",
                            AddressLine1 = "852 Poplar Drive",
                            AddressLine2 = "Apt 101",
                            ZipCode = "97204",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 62
                        },
                        new Address
                        {
                            FirstName = "Margaret",
                            LastName = "Taylor",
                            PhoneNumber = "555-0118",
                            AddressLine1 = "963 Hickory Court",
                            AddressLine2 = null,
                            ZipCode = "20001",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 8
                        },
                         new Address
                        {
                            FirstName = "Jessica",
                            LastName = "Miller",
                            PhoneNumber = "555-0108",
                            AddressLine1 = "258 Walnut Way",
                            AddressLine2 = null,
                            ZipCode = "30301",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 21
                        },
                        new Address
                        {
                            FirstName = "Robert",
                            LastName = "Davis",
                            PhoneNumber = "555-0109",
                            AddressLine1 = "369 Cherry Court",
                            AddressLine2 = "Floor 3",
                            ZipCode = "02108",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 55
                        },
                        new Address
                        {
                            FirstName = "Laura",
                            LastName = "Rodriguez",
                            PhoneNumber = "555-0110",
                            AddressLine1 = "741 Spruce Circle",
                            AddressLine2 = null,
                            ZipCode = "75201",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 89
                        },
                        new Address
                        {
                            FirstName = "William",
                            LastName = "Martinez",
                            PhoneNumber = "555-0111",
                            AddressLine1 = "852 Aspen Trace",
                            AddressLine2 = "Apt 12",
                            ZipCode = "80202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 15
                        },
                        new Address
                        {
                            FirstName = "Linda",
                            LastName = "Hernandez",
                            PhoneNumber = "555-0112",
                            AddressLine1 = "963 Willow Street",
                            AddressLine2 = null,
                            ZipCode = "19102",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 77
                        },
                        new Address
                        {
                            FirstName = "Richard",
                            LastName = "Lopez",
                            PhoneNumber = "555-0113",
                            AddressLine1 = "159 Cypress Avenue",
                            AddressLine2 = "Suite 5B",
                            ZipCode = "48226",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 38
                        },
                        new Address
                        {
                            FirstName = "Barbara",
                            LastName = "Gonzalez",
                            PhoneNumber = "555-0114",
                            AddressLine1 = "753 Redwood Road",
                            AddressLine2 = null,
                            ZipCode = "28202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 94
                        },
                        new Address
                        {
                            FirstName = "Joseph",
                            LastName = "Wilson",
                            PhoneNumber = "555-0115",
                            AddressLine1 = "456 Magnolia Blvd",
                            AddressLine2 = "Unit 90",
                            ZipCode = "37203",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 2
                        },
                        new Address
                        {
                            FirstName = "Susan",
                            LastName = "Anderson",
                            PhoneNumber = "555-0116",
                            AddressLine1 = "123 Sycamore Lane",
                            AddressLine2 = null,
                            ZipCode = "85004",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 49
                        },
                        new Address
                        {
                            FirstName = "Thomas",
                            LastName = "Thomas",
                            PhoneNumber = "555-0117",
                            AddressLine1 = "852 Poplar Drive",
                            AddressLine2 = "Apt 101",
                            ZipCode = "97204",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 62
                        },
                        new Address
                        {
                            FirstName = "Margaret",
                            LastName = "Taylor",
                            PhoneNumber = "555-0118",
                            AddressLine1 = "963 Hickory Court",
                            AddressLine2 = null,
                            ZipCode = "20001",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 8
                        },
                        new Address
                        {
                            FirstName = "Charles",
                            LastName = "Moore",
                            PhoneNumber = "555-0119",
                            AddressLine1 = "147 Chestnut Way",
                            AddressLine2 = "Room 304",
                            ZipCode = "46204",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 81
                        },
                        new Address
                        {
                            FirstName = "Lisa",
                            LastName = "Jackson",
                            PhoneNumber = "555-0120",
                            AddressLine1 = "258 Ash Street",
                            AddressLine2 = null,
                            ZipCode = "43215",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 27
                        },
                        new Address
                        {
                            FirstName = "Christopher",
                            LastName = "Martin",
                            PhoneNumber = "555-0121",
                            AddressLine1 = "369 Beech Avenue",
                            AddressLine2 = "Building A",
                            ZipCode = "78701",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 99
                        },
                        new Address
                        {
                            FirstName = "Nancy",
                            LastName = "Lee",
                            PhoneNumber = "555-0122",
                            AddressLine1 = "741 Alder Circle",
                            AddressLine2 = null,
                            ZipCode = "32202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 18
                        },
                        new Address
                        {
                            FirstName = "Daniel",
                            LastName = "Perez",
                            PhoneNumber = "555-0123",
                            AddressLine1 = "852 Dogwood Trace",
                            AddressLine2 = "Suite 400",
                            ZipCode = "76102",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 53
                        },
                        new Address
                        {
                            FirstName = "Karen",
                            LastName = "Thompson",
                            PhoneNumber = "555-0124",
                            AddressLine1 = "963 Holly Road",
                            AddressLine2 = null,
                            ZipCode = "38103",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 71
                        },
                        new Address
                        {
                            FirstName = "Matthew",
                            LastName = "White",
                            PhoneNumber = "555-0125",
                            AddressLine1 = "159 Juniper Blvd",
                            AddressLine2 = "Apt 22",
                            ZipCode = "40202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 36
                        },
                        new Address
                        {
                            FirstName = "Betty",
                            LastName = "Harris",
                            PhoneNumber = "555-0126",
                            AddressLine1 = "753 Larch Street",
                            AddressLine2 = null,
                            ZipCode = "21201",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 65
                        },
                        new Address
                        {
                            FirstName = "Anthony",
                            LastName = "Sanchez",
                            PhoneNumber = "555-0127",
                            AddressLine1 = "456 Olive Lane",
                            AddressLine2 = "Penthouse",
                            ZipCode = "53202",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 9
                        },
                        new Address
                        {
                            FirstName = "Helen",
                            LastName = "Clark",
                            PhoneNumber = "555-0128",
                            AddressLine1 = "123 Palm Drive",
                            AddressLine2 = null,
                            ZipCode = "68102",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 88
                        },
                        new Address
                        {
                            FirstName = "Mark",
                            LastName = "Ramirez",
                            PhoneNumber = "555-0129",
                            AddressLine1 = "852 Pecan Court",
                            AddressLine2 = "Unit B",
                            ZipCode = "27601",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 42
                        },
                        new Address
                        {
                            FirstName = "Sandra",
                            LastName = "Lewis",
                            PhoneNumber = "555-0130",
                            AddressLine1 = "963 Sequoia Way",
                            AddressLine2 = null,
                            ZipCode = "55402",
                            CreatedOnUtc = DateTime.UtcNow,
                            CityId = 100
                        }
                    }
                },
                new User
                {
                    Username = "testuser2@example.com",
                    Email = "testuser2@example.com",
                    FirstName = "Jane",
                    LastName = "Smith",
                    CreatedOnUtc = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true,
                    SecurityStamp = ""
                }
            };

            // Create roles if they don't exist
            var roleManager = _roleManager;

            var roles = new List<Role>
            {
                new Role { Name = UserRoleNameConstants.Guest, Active = true },
                new Role { Name = UserRoleNameConstants.Registered, Active = true }
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
            await _userManager.AddPasswordAsync(users[0], "Admin123-");
            await _userManager.AddToRoleAsync(users[0], UserRoleNameConstants.Registered);

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
                var approvedRatingSum = approvedReviewCount == 0
                    ? 0
                    : Random.Shared.Next(approvedReviewCount, approvedReviewCount * 5);
                products.Add(new Product
                {
                    Name = productName,
                    Description = description,
                    ShortDescription = shortDescription,
                    MaxAddToCartNumber = 350,
                    MinAddToCartNumber = Random.Shared.Next(1, 15),
                    Sku = $"SKU-{i.ToString().PadLeft(3, '0')}", // Create a SKU
                    IsAvailable = true,
                    IsPublished = true,
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
                    Brands = new List<ProductBrand>()
                    {
                        new ProductBrand
                        {
                            BrandId = brand.Id
                        }
                    },
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
                var productMedias = imageModels
                    .Select(x =>
                    {
                        return new ProductMedia
                        {
                            Product = products[i],
                            MediaFile = x,
                        };
                    })
                    .OrderBy(x => Guid.NewGuid().ToString())
                    .ToArray();
                productMedias.First().MainImage = true;
                _dbContext.ProductMedias.AddRange(productMedias);
            }


            //LABELS
            var refurbished = await _dbContext.Labels.SingleOrDefaultAsync(x => x.Name == "Recycling");
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
                var numSpecs = new Random().Next(2, 13); // Randomly assign between 2 and 4 specifications
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
                var numAttributes = 2; // Randomly assign between 3 and 5 attributes
                var selectedAttributes = productAttributes
                    .OrderBy(x => Guid.NewGuid())
                    .Take(numAttributes)
                    .ToList(); // Get unique options

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
                            Quantity = Random.Shared.Next(0, 3)
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
            .ToArray();

        int count = 0;
        foreach (var product in products)
        {
            count++;
            var productVariantAttributeList = productVariantAttributes
                .Where(x => x.ProductId == product.Id)
                .ToList();
            if (!productVariantAttributeList.Any()) continue; // Skip if no attributes


            decimal priceAdjustment = 0;
            decimal weightAdjustment = 0;
            var rawAttributes = new List<string>(); // Used to create raw attributes JSON
            // We need to create and assign more than one combination to a product and make sure that one combination doesn't contain all the variants options.

            var allValues = productVariantAttributeList.Select(x => x.ProductVariantAttributeValues);
            var result = CrossProduct(allValues);
            int count2 = 0;
            foreach (var pair in result)
            {
                count2++;
                var combination = new ProductVariantAttributeCombination
                {
                    ProductId = product.Id,
                    IsActive = Random.Shared.Next(0, 5) != 0,
                    Price = product.Price, // Start with product's base price
                    Weight = product.Weight, // Start with product's base weight
                    StockQuantity = count2 < 4 ? 0 : (count2 < 7 ? 15 : Random.Shared.Next(20, 300))
                };
                var selection = new ProductVariantAttributeSelection();
                foreach (var value in pair)
                {
                    selection.AddAttribute(value.ProductVariantAttributeId, value.Id);
                }

                combination.HashCode = selection.GetHashCode();
                combination.Price = decimal.Round(Random.Shared.NextDecimal(10, 900), 4);
                combination.Weight = decimal.Round(Random.Shared.NextDecimal(10, 900), 4);
                _dbContext.ProductVariantAttributeCombinations.Add(combination);
            }
        }

        await _dbContext.SaveChangesAsync();

        IEnumerable<IEnumerable<T>> CrossProduct<T>(
            IEnumerable<IEnumerable<T>> source) =>
            source.Aggregate(
                (IEnumerable<IEnumerable<T>>)new[] { Enumerable.Empty<T>() },
                (acc, src) => src.SelectMany(x => acc.Select(a => a.Concat(new[] { x }))));
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
                new Category
                    { Name = "Books", DisplayOrder = 3, Description = "Books category", IsPublished = true }
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

public static class Utils
{
    public static int NextInt32(this Random rng)
    {
        int firstBits = rng.Next(0, 1 << 4) << 28;
        int lastBits = rng.Next(0, 1 << 28);
        return firstBits | lastBits;
    }

    public static decimal NextDecimalSample(this Random random)
    {
        var sample = 1m;
        //After ~200 million tries this never took more than one attempt but it is possible to generate combinations of a, b, and c with the approach below resulting in a sample >= 1.
        while (sample >= 1)
        {
            var a = random.NextInt32();
            var b = random.NextInt32();
            //The high bits of 0.9999999999999999999999999999m are 542101086.
            var c = random.Next(542101087);
            sample = new Decimal(a, b, c, false, 28);
        }

        return sample;
    }

    public static decimal NextDecimal(this Random random)
    {
        return NextDecimal(random, decimal.MaxValue);
    }

    public static decimal NextDecimal(this Random random, decimal maxValue)
    {
        return NextDecimal(random, decimal.Zero, maxValue);
    }

    public static decimal NextDecimal(this Random random, decimal minValue, decimal maxValue)
    {
        var nextDecimalSample = NextDecimalSample(random);
        return maxValue * nextDecimalSample + minValue * (1 - nextDecimalSample);
    }
}