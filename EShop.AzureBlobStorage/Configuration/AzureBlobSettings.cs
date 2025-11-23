using EShop.Core.Platform.Configuration.Domain;

namespace EShop.AzureBlobStorage.Configuration;

public class AzureBlobSettings : ISettings
{
    public string ConnectionString { get; set; }
    public string ContainerName { get; set; }
    public string CdnEndpoint { get; set; }
}
 