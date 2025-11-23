using EShop.Core.Common.Domain;

namespace EShop.Core.Common.Services;

public interface IDeliveryTimeService
{
      Task<DeliveryTime?> GetDeliveryTimeAsync(int? deliveryTimeId, bool tracked = false);
      string? GetFormattedDeliveryDate(DeliveryTime? deliveryTime);
      ValueTuple<DateTime?, DateTime?> GetDeliveryDate(DeliveryTime? deliveryTime, DateTime fromDate);
}