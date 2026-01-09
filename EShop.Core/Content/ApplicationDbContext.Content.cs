using EShop.Core.Content.Media.Domain;
using EShop.Core.Content.Widgets.Domain;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

public partial class ApplicationDbContext
{
    public DbSet<Widget> Widgets { get; set; }
    public DbSet<WidgetInstance> WidgetInstances { get; set; }
    public DbSet<WidgetZone> WidgetZones { get; set; }
    public DbSet<MediaFile> MediaFiles { get; set; }
    public DbSet<ProductMedia> ProductMedias { get; set; }
}