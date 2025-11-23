using EShop.Core.Checkout.Shipping.Configuration;
using EShop.Core.Common.Domain;
using EShop.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Common.Services;

public class DeliveryTimeService : IDeliveryTimeService
{
    private readonly ApplicationDbContext _db;
    private readonly ShippingSettings _shippingSettings;
    private const string DateSpecifier = "dddd, MMMM d";
    private const string HtmlFormation = "<time>{0}</time> - <time>{1}</time>";
    private const string HtmlFormationSingle = "<time>{0}</time>";

    public DeliveryTimeService(ApplicationDbContext db, ShippingSettings shippingSettings)
    {
        _db = db;
        _shippingSettings = shippingSettings;
    }

    public async Task<DeliveryTime> GetDeliveryTimeAsync(int? deliveryTimeId, bool tracked = false)
    {
        if (deliveryTimeId == 0 || deliveryTimeId == null)
            return null;

        var query = _db.DeliveryTimes.Where(x => x.Id == deliveryTimeId.Value);
        query = tracked ? query : query.AsNoTracking();
        return await query.FirstOrDefaultAsync();
    }

    public string? GetFormattedDeliveryDate(DeliveryTime? deliveryTime)
    {
        if (deliveryTime is null || (!deliveryTime.MinDays.HasValue && !deliveryTime.MaxDays.HasValue))
        {
            return null;
        }

        // simplified cuz ideally we'd have to let the administrator specify the time zone of the store,
        // and then we'd have to also ask the customer about their time zone and then convert min and max into the customer's  time zone using TimeZoneInfo.ConvertTime.
        // In my case I am just going to use the local time zone.
        var currentDateTime = DateTime.Now;
        var (min, max) = GetDeliveryDate(deliveryTime, currentDateTime);
        if (min.HasValue && max.HasValue)
        {
            string minString = min.Value.ToString(DateSpecifier);

            if (min == max)
            {
                return string.Format(HtmlFormationSingle, minString);
            }

            string maxString = max.Value.ToString(DateSpecifier);
            return string.Format(HtmlFormation, minString, maxString);
        }
        else
        {
            return null;
        }
    }


    public ValueTuple<DateTime?, DateTime?> GetDeliveryDate(DeliveryTime? deliveryTime, DateTime fromDate)
    {
        var minDate = deliveryTime?.MinDays != null
            ? AddBusinessDays(fromDate, deliveryTime.MinDays.Value)
            : (DateTime?)null;

        var maxDate = deliveryTime?.MaxDays != null
            ? AddBusinessDays(fromDate, deliveryTime.MaxDays.Value)
            : (DateTime?)null;
        return new ValueTuple<DateTime?, DateTime?>(minDate, maxDate);
    }

    private DateTime AddBusinessDays(DateTime date, int days)
    {
        if (date.Hour < _shippingSettings.TodayShipmentHour)
        {
            if ((date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday) ||
                !_shippingSettings.DeliveryOnWorkweekDaysOnly)
            {
                // If that's the case, it means that today is counted, and we've got to take away one day. 
                days -= 1;
            }
        }

        if (days < 1)
        {
            days = 1;
        }

        if (!_shippingSettings.DeliveryOnWorkweekDaysOnly)
        {
            return date.AddDays(days);
        }

        //If it's saturday, we want to step over to Monday subtracting one day for that Monday. 
        if (date.DayOfWeek == DayOfWeek.Saturday)
        {
            date = date.AddDays(2);
            days -= 1;
        }
        else if (date.DayOfWeek == DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
            days -= 1;
        }

        // if the days value is less than 5 days, we don't do here anything cuz we don't want to add one week on top.
        // If the days is 5 days, we add exactly one week (because we want to include the weekend). 5 + 2 = 7 days. 
        // if it's more that 5 days, but less than 10 we do the same and collect the weekend days (if any) in the next line.
        // If it's 10 days, we add two weeks (14 days) cuz 5 + 5 + 4 (two Sundays and two Saturdays) = 14 days. 
        date = date.AddDays(days / 5 * 7);

        // We work out days mod 5 to see how many weekend days are inside. 
        // For example, if we had 7 days left in daysToAdd, the result would be 2,
        // which is the Sunday and Saturday and are inside these 7 days.
        int extraDays = days % 5;

        //TODO: Look into this: 
        //If DayOfWeek + extraDays is more than 5, it means that we automatically want to just over to the next Monday.
        //That is why we will always want to add 2 days because no matter if it is sunday or saturday, we still want to jump over to  Monday
        if ((int)date.DayOfWeek + extraDays > 5)
        {
            //TODO: Look into this: 
            //if it is Friday, which is 5 in the enum we use, it means tha we have to just over 4 days (or whatever),
            //so like if it is friday, then friday => saturday => sunday => monday
            extraDays += 2;
        }

        return date.AddDays(extraDays);
    }
}