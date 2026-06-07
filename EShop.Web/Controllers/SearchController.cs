// using EShop.Core.Data.Search.Domain;
// using EShop.Core.Data.Search.Services;
// using EShop.Web.Common.Controllers;
// using Microsoft.AspNetCore.Mvc;
//
// namespace EShop.Web.Controllers;
//
// public class SearchController : EShopBaseController
// {
//     private readonly SearchSettings _searchSettings;
//
//     public SearchController(SearchSettings searchSettings)
//     {
//         _searchSettings = searchSettings;
//     }
//
//     public async Task<IActionResult> InstanceSearch(CatalogSearchQuery query)
//     {
//
//         query.Slice(0, Math.Min(15, _searchSettings.InstantSearchMaxResultNumber));
//         var result = await 
//     }
// }