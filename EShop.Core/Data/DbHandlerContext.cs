using EShop.Core.Data.DbHandlers;
using EShop.Infrastructure.Engine;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Data;

public class DbHandlerContext : DbContext
{
     
    
    public DbHandlerContext(DbContextOptions<DbHandlerContext> options) : base(options)
    {
       
    }

    
}