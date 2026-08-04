// using EShop.Core.Platform.Identity.Domain;
// using EShop.Infrastructure.Domain;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
//
// namespace EShop.Core.Common.Domain;
//
// internal class ProductLinkMap : IEntityTypeConfiguration<UserAddress>
// {
//     public void Configure(EntityTypeBuilder<UserAddress> builder)
//     {
//       
//     }
// }
//
// public class UserAddress : BaseEntity
// {
//     public AddressType AddressType { get; set; }
//     public int UserId { get; set; }
//     public int AddressId { get; set; }
//     public User User { get; set; }
//     public Address Address { get; set; }
// }