using EShop.Core.Platform.Themes.Domain;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

public partial class ApplicationDbContext 
{
    public DbSet<ThemeVariable> ThemeVariables { get; set; }
}