using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Logging.Domain;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Platform.Logging.Services;

public interface IActivityLogger
{
    ActivityLog InsertActivity(string systemKeyword, string comment, params object[] commentParams);

    ActivityLog InsertActivity(User user, string systemKeyword, string comment,
        params object[] commentParams);

    ActivityLogType GetActivityTypeByKeyword(string keyword);
}