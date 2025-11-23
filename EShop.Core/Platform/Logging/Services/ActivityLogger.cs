using System.Globalization;
using EShop.Core.Data;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Logging.Domain;
using EShop.Infrastructure.Domain;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using EShop.Infrastructure.Extensions;

namespace EShop.Core.Platform.Logging.Services;

public class ActivityLogger : IActivityLogger
{
    private readonly ApplicationDbContext _db;
    private readonly IWorkContext _workContext;

    public ActivityLogger(ApplicationDbContext db, IWorkContext workContext)
    {
        _db = db;
        _workContext = workContext;
    }

    public ActivityLog InsertActivity(string systemKeyword, string comment, params object[] commentParams)
    {
        return InsertActivity(_workContext.CurrentUser, systemKeyword, comment, commentParams);
    }

    public ActivityLog InsertActivity(User user, string systemKeyword, string comment,
        params object[] commentParams)
    {
        if (user is null)
        {
            return null;
        }

        var activityT = GetActivityTypeByKeyword(systemKeyword);
        if (!activityT?.Enabled ?? true)
        {
            return null;
        }
         

        string commentStr = string
            .Format(CultureInfo.CurrentCulture, comment.EmptyIfNull(), commentParams)
            .Reduce(4000);

        var activityLog = new ActivityLog
        {
            ActivityLogTypeId = activityT.Id,
            UserId = user.Id,
            Comment = commentStr,
            CreatedOnUtc = DateTime.UtcNow,
        };

        _db.ActivityLogs.Add(activityLog);
        return activityLog;
    }


    public ActivityLogType GetActivityTypeByKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return null;
        }

        if (GetCachedActivityLogTypes()
            .TryGetValue(keyword, out var logType))
        {
            return logType;
        }

        return null;
    }

    private IReadOnlyDictionary<string, ActivityLogType> GetCachedActivityLogTypes()
    {
        //TODO: add caching. 
        var allTypes = _db
            .ActivityLogTypes
            .AsNoTracking()
            .ToList();

        return allTypes.ToDictionary(x => x.SystemKeyword, x => x);
    }
}