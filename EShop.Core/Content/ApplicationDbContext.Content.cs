using EShop.Core.Content.Media.Domain;
 
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

public partial class ApplicationDbContext
{
    
    public DbSet<MediaFile> MediaFiles { get; set; }
    public DbSet<ProductMedia> ProductMedias { get; set; }
}