using EShop.Core.Content.Widgets.Domain;
using EShop.Core.Data;

namespace EShop.Core.Content.Widgets.Services;

public class WidgetInstanceService : IWidgetInstanceService
{
    private readonly ApplicationDbContext _db;

    public WidgetInstanceService(ApplicationDbContext db)
    {
        _db = db;
    }


    public IQueryable<WidgetInstance> GetPublishedWidgetQuery()
    {
        return _db.WidgetInstances
            .Where(x => x.PublishStartUtc.HasValue && x.PublishStartUtc.Value < DateTime.UtcNow &&
                        (!x.PublishEndUtc.HasValue || x.PublishEndUtc.Value > DateTime.UtcNow));
    }
}