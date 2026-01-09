using EShop.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Platform.Routing;

public class UrlService : IUrlService
{
    private readonly ApplicationDbContext _context;

    public UrlService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetActiveSlugAsync(int entityId, string entityName)
    {
        ArgumentNullException.ThrowIfNull(entityName);
        
        //TODO: do prefetching.
        //TODO: do caching.

        //
        // var urlRecords = await _context
        //     .UrlRecords.AsNoTracking()
        //     .Where(x => x.Active)
        //     .OrderByDescending(x => x.Id)
        //     .ToListAsync();
        //
        // Dictionary<string, string> allActiveSlugs = urlRecords.ToDictionary(x => GenerateKey(x.EntityId, x.EntityName),
        //     x => x.Slug,
        //     StringComparer.OrdinalIgnoreCase);

        return await _context
            .UrlRecords.AsNoTracking()
            .Where(x => x.EntityId == entityId && x.EntityName == entityName)
            .Where(x => x.Active)
            .OrderByDescending(x => x.Id)
            .Select(x => x.Slug)
            .FirstOrDefaultAsync();
        
    }
}