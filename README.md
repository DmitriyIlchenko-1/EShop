# EShop
EShop is a modular, scalable eCommerce web application built with `ASP.NET Core 10`, `Entity Framework Core 10`, `JavaScript`, `CSS/HTML`, and more. 
The project includes all the necessary features such as Cart, authorization with external Auth providers like Google, Price management, Cache & cache invalidation, Theme management.

## Key Features 
- Product variants, combinations, attributes, specification
- Extendable price calculators that take into account product discounts etc
- Reviews and ratings
- User management

### Repository Structure
| Project                                                                                                         | Description                                                                                                                                                                                                          |
|-----------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [`EShop.Insfactructure`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.Infrastructure)           | Third party API integrations, cache management, utility types, bootstrapping types, extension methods, module management, and other insfactructure concerns                                                          |
| [`EShop.Core`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.Core)                               | Domain concerns like domain entities, services, business rules, price calculation logic, settings, configurations and other domain parts of the app like Themes, Logging, Identity, Shipping, Checkout, Catalog etc. |
| [`EShop.Web.Framework`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.Web.Framework)             | Reusable types & features for the front-end development like tag helpers, base controller & component types, Razor locator expanders for themes and partial views. Razor template rendering extensions.              |
| [`EShop.ExternalAuth.Google`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.ExternalAuth.Google) | Module implementing authorization with Google as an external Auth provider                                                                                                                                           |
| [`EShop.OfflinePayment`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.OfflinePayment)           | Module implementing offline payment method. Currently it's just for demonstrations of modularity. It doesn't do anything.                                                                                            |
| [`EShop.Web`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.Web)                                 | Startup project: controllers, views, mappers, mapping factories, view models (DTOs), migrations, themes, static files (JS, CSS, HTML, images)                                                                        |
| [`EShop.Core.Tests`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.Core.Tests)                   | Unit tests for the Core project                                                                                                                                                                                      |
| [`EShop.Tests`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.Tests)                             | Unit tests for infrastructure concerns like Type scanner                                                                                                                                                             |
| [`EShop.Tests.Framework`](https://github.com/DmitriyIlchenko-1/EShop/tree/master/EShop.Tests.Framework)         | Utility types and helper methods used by other unit test project                                                                                                                    |



## Get started
All you need to run the project is to build the solution to have the module dll files copied into the Module folder in the `EShop.Web` project and fill out `appsettings.json` with necessary data like your database connection and cache configuration settings. PostgreSQL is the only supported provider.
