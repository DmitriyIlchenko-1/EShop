using Bogus;
using EShop.Web.Data; // Replace with the namespace where your DbContext and Entities are
using EShop.Web.Models; // Replace with the namespace where your DbContext and Entities are
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography; // For password hashing example
using System.Text;
using System.Text.RegularExpressions;
using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Common.Domain;
using EShop.Core.Content.Media.Domain;
using EShop.Core.Content.Widgets.Domain;
using EShop.Core.Data;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Routing.Domain;
using Microsoft.AspNetCore.Identity;

namespace EShop.Web.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        var userManager = app
            .Services.CreateScope()
            .ServiceProvider.GetRequiredService<UserManager<User>>();
        var db = app
            .Services.CreateScope()
            .ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Use a consistent seed for reproducible data
        Randomizer.Seed = new Random(8675309);
        var faker = new Faker("en");

        // The order of these calls is crucial to respect foreign key constraints.
        await SeedIndependentEntities(db, faker);
        await SeedUsersAndRoles(db, userManager, faker);
        await SeedLocations(db, faker);
        await SeedCatalogCore(db, faker);
        await SeedProductsAndDependencies(db, faker);
    }

    // These entities have no or few dependencies on others
    private static async Task SeedIndependentEntities(ApplicationDbContext context, Faker faker)
    {
        if (await context.Brands.AnyAsync()) return; // Assume if one is populated, all are.

        // Catalog_Brand
        var brands = new Faker<Brand>()
            .RuleFor(b => b.Name, f => f.Company.CompanyName())
            .RuleFor(b => b.Description, f => f.Company.CatchPhrase())
            .RuleFor(b => b.IsPublished, true)
            .RuleFor(b => b.Deleted, false)
            .Generate(15);
        await context.Brands.AddRangeAsync(brands);

        // Content_Media (for product thumbnails)
        var media = new Faker<Media>()
            .RuleFor(m => m.Filename, f => f.System.FileName("jpg"))
            .RuleFor(m => m.UploadedAtUtc,
                f => f
                    .Date.Past(2)
                    .ToUniversalTime())
            .Generate(100); // More media than products
        await context.Medias.AddRangeAsync(media);

        // Common_DeliveryTime
        var deliveryTimes = new List<DeliveryTime>
        {
            new DeliveryTime
            {
                Name = "1-3 Business Days", DisplayOrder = 1, IsDefault = true, MinDays = 1, MaxDays = 3,
                ColorHexValue = "#00FF00", DisplayLocate = "en-US"
            },
            new DeliveryTime
            {
                Name = "3-5 Business Days", DisplayOrder = 2, IsDefault = false, MinDays = 3, MaxDays = 5,
                ColorHexValue = "#FFA500", DisplayLocate = "en-US"
            },
            new DeliveryTime
            {
                Name = "5-10 Business Days", DisplayOrder = 3, IsDefault = false, MinDays = 5, MaxDays = 10,
                ColorHexValue = "#FF0000", DisplayLocate = "en-US"
            }
        };
        await context.DeliveryTimes.AddRangeAsync(deliveryTimes);

        // Content_Widget (As requested)
        var widgets = new List<Widget>
        {
            new Widget("HtmlWidget")
            {
                Name = "Html Widget", CreateUrl = "/Admin/HtmlWidget/Create", EditUrl = "/Admin/HtmlWidget/Edit/{0}",
                IsPublished = true, ViewComponentName = "HtmlWidget", CreatedOnUtc = DateTime.UtcNow
            },
            new Widget("CarouselWidget")
            {
                Name = "Carousel Widget", CreateUrl = "/Admin/CarouselWidget/Create",
                EditUrl = "/Admin/CarouselWidget/Edit/{0}", IsPublished = true, ViewComponentName = "CarouselWidget",
                CreatedOnUtc = DateTime.UtcNow
            },
            new Widget("SpaceBarWidget")
            {
                Name = "SpaceBar Widget", CreateUrl = "/Admin/SpaceBarWidget/Create",
                EditUrl = "/Admin/SpaceBarWidget/Edit/{0}", IsPublished = true, ViewComponentName = "SpaceBarWidget",
                CreatedOnUtc = DateTime.UtcNow
            }
        };
        await context.Widgets.AddRangeAsync(widgets);

        // Content_WidgetZone
        var widgetZones = new List<WidgetZone>
        {
            new WidgetZone
                { Name = "Home Page - Before Content", Description = "Appears at the top of the home page." },
            new WidgetZone
                { Name = "Home Page - After Content", Description = "Appears at the bottom of the home page." },
            new WidgetZone
                { Name = "Product Details - Sidebar", Description = "Appears in the sidebar on product detail pages." }
        };
        await context.WidgetZones.AddRangeAsync(widgetZones);

        await context.SaveChangesAsync(); // Save to get IDs for dependent entities
    }

    private static async Task SeedUsersAndRoles(ApplicationDbContext context, UserManager<User> userManager,
        Faker faker)
    {
        if (await context.Users.AnyAsync()) return;


        // Platform_Role
        var roles = new List<Role>
        {
            new Role { Name = "Admin", NormalizedName = "ADMIN", Active = true },
            new Role { Name = UserRoleNameConstants.Guest, NormalizedName = "GUEST", Active = true },
            new Role { Name = UserRoleNameConstants.Registered, NormalizedName = "REGISTERED", Active = true }
        };
        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();


        // Platform_User
        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
        var customerRole = await context.Roles.FirstAsync(r => r.Name == UserRoleNameConstants.Registered);

        var user = new User
        {
            FirstName = "Test User",
            LastName = "Test User",
            Email = "test@test.com",
            UserName = "test",
            PhoneNumber = "123",
            UserGuid = Guid.NewGuid(),
            LastVisitedPage = "/",
            CreatedOnUtc = DateTime.UtcNow,
            LastActivityDateUtc = DateTime.UtcNow,
            ClientIdentity = "test-identity",
            LastIpAddress = "127.0.0.1",
            LastUserAgent = "test-user",
            LastUserDeviceType = "phone",
            UserRoles = new List<UserRole>()
            {
                new UserRole()
                {
                    RoleId = adminRole.Id,
                }
            }
        };
        await userManager.CreateAsync(user, "123");


        // Note: In a real app, use ASP.NET Core Identity's PasswordHasher.
        // This is a simplified placeholder.
        var passwordHash = "AQAAAAEAACcQAAAAE... (A real hash would go here)";

        var users = new Faker<User>()
            .RuleFor(u => u.UserName, f => f.Internet.UserName(f.Name.FirstName(), f.Name.LastName()))
            .RuleFor(u => u.FirstName, f => f.Internet.UserName(f.Name.FirstName(), f.Name.LastName()))
            .RuleFor(u => u.LastName, f => f.Internet.UserName(f.Name.FirstName(), f.Name.LastName()))
            .RuleFor(u => u.NormalizedUserName, (f, u) => u.UserName.ToUpper())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.UserName))
            .RuleFor(u => u.NormalizedEmail, (f, u) => u.Email.ToUpper())
            .RuleFor(u => u.EmailConfirmed, true)
            .RuleFor(u => u.PasswordHash, passwordHash)
            .RuleFor(u => u.SecurityStamp,
                f => Guid
                    .NewGuid()
                    .ToString())
            .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(u => u.PhoneNumberConfirmed, true)
            .RuleFor(u => u.FirstName, f => f.Name.FullName())
            .RuleFor(u => u.LastName, f => f.Name.FullName())
            .RuleFor(u => u.UserGuid, f => Guid.NewGuid())
            .RuleFor(u => u.CreatedOnUtc,
                f => f
                    .Date.Past(3)
                    .ToUniversalTime())
            .RuleFor(u => u.LastActivityDateUtc,
                f => f
                    .Date.Recent()
                    .ToUniversalTime())
            .RuleFor(u => u.LatestUpdateOnUtc,
                f => f
                    .Date.Recent()
                    .ToUniversalTime())
            .RuleFor(u => u.Active, true)
            .RuleFor(u => u.IsDeleted, false)
            .RuleFor(u => u.LastIpAddress, f => f.Internet.Ip())
            .FinishWith((f, u) =>
            {
                // Assign roles
                var userRole = new UserRole { Role = f.PickRandom(new[] { adminRole, customerRole }) };
                u.UserRoles.Add(userRole);
            })
            .Generate(25);

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }

    private static async Task SeedLocations(ApplicationDbContext context, Faker faker)
    {
        if (await context.Cities.AnyAsync()) return;

        // Common_City & Common_District
        for (int i = 0; i < 10; i++)
        {
            var city = new City
            {
                Name = faker.Address.City(),
                IsBillingEnabled = true,
                IsShippingEnabled = true,
                IsCityEnabled = true,
                IsZipCodeEnabled = true
            };
            context.Cities.Add(city);
            await context.SaveChangesAsync(); // Save to get City Id

            var districts = new Faker<District>()
                .RuleFor(d => d.Name, f => f.Address.State() + " District") // States make good district names
                .RuleFor(d => d.CityId, city.Id)
                .Generate(faker.Random.Int(3, 8));
            await context.Districts.AddRangeAsync(districts);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedCatalogCore(ApplicationDbContext context, Faker faker)
    {
        if (await context.Categories.AnyAsync()) return;

        // Catalog_Category (Parent and Child)
        var parentCategories = new Faker<Category>()
            .RuleFor(c => c.Name, f => f.Commerce.Department(1))
            .RuleFor(c => c.Slug, (f, c) => GenerateSlug(c.Name))
            .RuleFor(c => c.Description, f => f.Lorem.Sentence())
            .RuleFor(c => c.MetaTitle, (f, c) => c.Name)
            .RuleFor(c => c.MetaDescription, (f, c) => c.Description)
            .RuleFor(c => c.IsPublished, true)
            .RuleFor(c => c.IncludeInMenu, true)
            .Generate(5);
        await context.Categories.AddRangeAsync(parentCategories);
        await context.SaveChangesAsync();

        foreach (var parent in parentCategories)
        {
            var childCategories = new Faker<Category>()
                .RuleFor(c => c.Name,
                    f => f
                        .Commerce.Categories(1)[0])
                .RuleFor(c => c.Slug, (f, c) => GenerateSlug(c.Name))
                .RuleFor(c => c.Description, f => f.Lorem.Sentence())
                .RuleFor(c => c.MetaTitle, (f, c) => c.Name)
                .RuleFor(c => c.MetaDescription, (f, c) => c.Description)
                .RuleFor(c => c.IsPublished, true)
                .RuleFor(c => c.IncludeInMenu, true)
                .RuleFor(c => c.ParentId, parent.Id)
                .Generate(faker.Random.Int(2, 5));
            await context.Categories.AddRangeAsync(childCategories);
        }

        // Catalog_SpecificationAttribute & Options
        var specAttributeNames = new[] { "Processor", "RAM", "Storage", "Screen Size", "Color" };
        foreach (var name in specAttributeNames)
        {
            var specAttr = new SpecificationAttribute
            {
                Name = name,
                Alias = name,
                AllowFiltering = true,
                ShowOnProductPage = true,
                IsEssential = faker.Random.Bool(),
                DisplayOrder = 1
            };
            context.SpecificationAttributes.Add(specAttr);
            await context.SaveChangesAsync(); // Save to get Id

            var options = new Faker<SpecificationAttributeOption>()
                .RuleFor(o => o.Name, f => f.Commerce.ProductAdjective())
                .RuleFor(o => o.Color, f => f.Internet.Color())
                .RuleFor(o => o.SpecificationAttributeId, specAttr.Id)
                .Generate(4);
            await context.SpecificationAttributeOptions.AddRangeAsync(options);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAndDependencies(ApplicationDbContext context, Faker faker)
    {
        if (await context.Products.AnyAsync()) return;

        var brands = await context.Brands.ToListAsync();
        var media = await context.Medias.ToListAsync();
        var users = await context.Users.ToListAsync();
        var allCategories = await context
            .Categories.Where(c => c.ParentId != null)
            .ToListAsync();
        var specOptions = await context.SpecificationAttributeOptions.ToListAsync();

        // Catalog_Product
        var products = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.ShortDescription, f => f.Commerce.ProductAdjective() + " " + f.Lorem.Sentence(5))
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Sku, f => f.Commerce.Ean13())
            .RuleFor(p => p.Gtin, f => f.Commerce.Ean8())
            .RuleFor(p => p.Price, f => Math.Round(f.Random.Decimal(10, 1000), 2))
            .RuleFor(p => p.OldPrice, (f, p) => Math.Round(p.Price * f.Random.Decimal(1.1m, 1.5m), 2))
            .RuleFor(p => p.StockQuantity, f => f.Random.Int(0, 100))
            .RuleFor(p => p.BrandId,
                f => f.PickRandom(brands)
                    .Id)
            .RuleFor(p => p.ThumbnailImageId,
                f => f.PickRandom(media)
                    .Id)
            .RuleFor(p => p.CreatedOnUtc,
                f => f
                    .Date.Past(1)
                    .ToUniversalTime())
            .RuleFor(p => p.MetaTitle, (f, p) => p.Name)
            .RuleFor(p => p.MetaDescription, (f, p) => p.ShortDescription)
            .Generate(50);
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Dependencies that require Product IDs

        var productCategories = new List<ProductCategory>();
        var productSpecs = new List<ProductSpecificationAttribute>();
        var productReviews = new List<ProductReview>();

        foreach (var product in products)
        {
            // Catalog_ProductCategory
            productCategories.Add(new ProductCategory
            {
                ProductId = product.Id, CategoryId = faker.PickRandom(allCategories)
                    .Id
            });

            // Catalog_ProductSpecificationAttribute
            productSpecs.Add(new ProductSpecificationAttribute
            {
                ProductId = product.Id, SpecificationAttributeOptionId = faker.PickRandom(specOptions)
                    .Id
            });

            // Catalog_ProductReview (and Catalog_Reply)
            if (faker.Random.Bool(0.7f))
            {
                var review = new ProductReview
                {
                    ProductId = product.Id,
                    UserId = faker.PickRandom(users)
                        .Id,
                    Title = faker.Lorem.Sentence(4),
                    CommentText = faker.Lorem.Paragraph(),
                    Rating = faker.Random.Int(3, 5),
                    ReviewerName = faker.Name.FullName(),
                    CreatedOnUtc = faker
                        .Date.Past()
                        .ToUniversalTime(),
                    ReviewStatus = ReviewStatus.Approved // Approved
                };

                // Add a reply to some reviews
                if (faker.Random.Bool(0.4f))
                {
                    review.Replies = new List<Reply>
                    {
                        new Reply
                        {
                            UserId = faker.PickRandom(users)
                                .Id,
                            ReplyText = faker.Lorem.Sentence(),
                            ReplierName = "Store Manager",
                            CreatedOnUtc = DateTime.UtcNow,
                            ReplyStatus = ReplyStatus.Approved // Approved
                        }
                    };
                }

                productReviews.Add(review);
            }
        }

        await context.ProductCategories.AddRangeAsync(productCategories);
        await context.ProductSpecificationAttributes.AddRangeAsync(productSpecs);
        await context.ProductReviews.AddRangeAsync(productReviews);

        // Platform_EntityType & Platform_UrlRecord (As requested)
        if (!await context.EntityTypes.AnyAsync(et => et.Id == "Product"))
        {
            var entityType = new EntityType
            {
                Id = "Product",
                TargetActionName = "ProductDetails",
                TargetControllerName = "Product",
                TargetAreaName = "" // Assuming no area
            };
            context.EntityTypes.Add(entityType);
            await context.SaveChangesAsync();
        }

        var productEntityType = await context.EntityTypes.FirstAsync(et => et.Id == "Product");
        var urlRecords = products
            .Select(p => new UrlRecord
            {
                EntityId = p.Id,
                EntityTypeId = productEntityType.Id,
                Name = p.Name,
            })
            .ToList();

        await context.UrlRecords.AddRangeAsync(urlRecords);

        await context.SaveChangesAsync();
    }

    private static string GenerateSlug(string phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase))
            return string.Empty;

        // 1. Convert to lower case
        string str = phrase.ToLowerInvariant();

        // 2. Remove invalid characters
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

        // 3. Convert multiple spaces into one space and trim
        str = Regex
            .Replace(str, @"\s+", " ")
            .Trim();

        // 4. Replace spaces with hyphens
        str = str.Replace(" ", "-");

        // 5. Ensure no double-hyphens
        str = Regex.Replace(str, @"-+", "-");

        return str;
    }
}