# EShop

EShop is a modular, scalable eCommerce web application built with `ASP.NET Core 10`, `Entity Framework Core 10`, `AngleSharp`, `PostgreSQL`,
`FluentValidation`, `Autofac`, `EasyCaching`, `JavaScript`, `CSS/HTML`, and more.
The project includes all the necessary features such as product attribute and combination management, Cart,
authorization with external Auth providers like Google, Price management, Cache & cache invalidation, Theme management.

## Key Features

- **Product management:** Product variants, combinations, attributes, specification
- **Price calculators**: Extendable price calculators that take into account product discounts etc
- **Reviews and ratings**
- **Cache + cache invalidation for stale data**
- **EF Core utilities:** EF Core utility classes allowing to make efficient requests to the database
- **SEO Friendly:** Support for SEO friendly names for products, categories and other domain entities.
- **User management:** Login, External Login (e.g. through Google), External Login Correlation (to request more data from user), Registration, Email confirmation, Password Change, Password Recovery, Cart Migration
- **Image resizing:** There's an image resizing middleware serving optimal size images that are first downsized and then cached to serve them fast to
  the user. Thanks to the HTML srcset attribute. 
- **Notification support:** Notification service to display alerts to the user sent directly from the server

### Repository Structure

| Project                                                                                                         | Description                                                                                                                                                                                                          |
|-----------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [`EShop.Insfactructure`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.Infrastructure)           | Third party API integrations, cache management, utility types, bootstrapping types, extension methods, module management, and other infrastructure concerns                                                          |
| [`EShop.Core`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.Core)                               | Domain concerns like domain entities, services, business rules, price calculation logic, settings, configurations and other domain parts of the app like Themes, Logging, Identity, Shipping, Checkout, Catalog etc. |
| [`EShop.Web.Framework`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.Web.Framework)             | Reusable types & features for the front-end development like tag helpers, base controller & component types, Razor locator expanders for themes and partial views. Razor template rendering extensions.              |
| [`EShop.ExternalAuth.Google`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.ExternalAuth.Google) | Module implementing authorization with Google as an external Auth provider                                                                                                                                           |
| [`EShop.OfflinePayment`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.OfflinePayment)           | Module implementing offline payment method. Currently it's just for demonstrations of modularity. It doesn't do anything.                                                                                            |
| [`EShop.Web`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.Web)                                 | Startup project: controllers, views, mappers, mapping factories, view models (DTOs), migrations, themes, static files (JS, CSS, HTML, images)                                                                        |
| [`EShop.Core.Tests`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.Core.Tests)                   | Unit tests for the Core project                                                                                                                                                                                      |
| [`EShop.Tests`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.Tests)                             | Unit tests for infrastructure concerns like Type scanner                                                                                                                                                             |
| [`EShop.Tests.Framework`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.Tests.Framework)         | Utility types and helper methods used by other unit test project                                                                                                                                                     |

## Get started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Any code editor (Visual Studio, VS Code, JetBrains Rider)
 

1. **Clone the repository**:

   ```bash
   git clone https://github.com/DmitriyIlchenko-1/EShop.git
   cd EShop
   ```
2. **Build _the solution_ to have the module dll files copied into the Module folder in the `EShop.Web` project**:

   ```bash
   dotnet build
   ```

3. **Choose the startup project - EShop.Web**:

   ```bash
   cd EShop.Web
   ```
4. **Set up the database connection string in `appsettings.json`** (a sample string):
    ```json
    "DbConnections:DefaultDbConnection": "User ID=postgres; Password=[password]; Host=localhost; Port=[port]; Database=EShopDatabase; Connection Lifetime=0; Include Error Detail=true",
    ```
   PostgreSQL is the only supported provider. Also, you don't have to provide a Redis configuration, in which case, the in-memory cache will be used for all cache operations even where a distributed cache is requested.

5. **Run the application. Use these credentials to freely make purchases on the website. _The app's case-sensitive!_** :
    ```json
    "Username": "admin123",
    "Email": "admin@gmail.com",
    "Password": "Admin123-"
    ```
 

 
