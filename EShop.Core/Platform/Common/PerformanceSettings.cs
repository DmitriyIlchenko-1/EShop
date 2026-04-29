using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Platform.Common;

public class PerformanceSettings : ISettings
{
    public int MaxUnavailableCombinations { get; set; }
}