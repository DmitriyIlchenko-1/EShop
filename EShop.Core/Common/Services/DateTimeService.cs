namespace EShop.Core.Common.Services;

public class DateTimeService : IDateTimeService
{
    /// <summary>
    /// Local time zone is the time zone set on the device the backend is running at.
    /// This is the time zone the store uses when displaying stuff to the User.
    /// </summary>
    /// <param name="dateTimeUtc"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public DateTime ConvertToLocalTimeZoneFromUtc(DateTime dateTimeUtc)
    {
        if (dateTimeUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("DateTimeKind is not Utc", nameof(dateTimeUtc));

        var result = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, TimeZoneInfo.Local);

        return result.Kind != DateTimeKind.Local ? DateTime.SpecifyKind(result, DateTimeKind.Local) : result;
    }
}