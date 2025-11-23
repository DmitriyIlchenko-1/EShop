// using System.Linq.Expressions;
// using EShop.Infrastructure.Domain;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata;
//
// namespace EShop.Infrastructure.Data;
//
// public static class EfCoreExtensions
// {
//      public static IQueryable<T> SelectForSummary<T>(this IQueryable<T> query) where T : BaseEntity
//      {
//           ArgumentNullException.ThrowIfNull(query);
//           
//      }
//
//      private static Expression<Func<T, T>> GetEntityForSummarySelector<T>(IModel entityModel) where T : BaseEntity
//      {
//           
//      }
// }