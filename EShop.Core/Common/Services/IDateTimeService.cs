namespace EShop.Core.Common.Services;

public interface IDateTimeService
{
    DateTime ConvertToLocalTimeZoneFromUtc(DateTime dateTimeUtc);
}