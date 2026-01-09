using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EShop.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;

namespace EShop.AzureBlobStorage.Providers
{
    public class AzureBlobStorageProvider : IMediaStorageProvider
    {
        private readonly BlobContainerClient _blobContainerClient;
        private string _CdnEndpoint;

        public AzureBlobStorageProvider(IConfiguration configuration)
        {
            string connectionString = configuration["Azure:BlobStorage:ConnectionString"];
            string containerName = configuration["Azure:BlobStorage:ContainerName"];
            string endpoint = configuration["Azure:BlobStorage:CdnEndpoint"];

            _CdnEndpoint = endpoint;
            _blobContainerClient = (new BlobServiceClient(connectionString)).GetBlobContainerClient(containerName);
        }

        public async Task DeleteMediaAsync(string fileName)
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        public Task<string> GetMediaUrlAsync(string fileName)
            => Task.FromResult($"{_CdnEndpoint}/{_blobContainerClient.Name}/{fileName}");


        public async Task SaveMediaAsync(Stream mediaBinaryStream, string fileName, string mimeType = null)
        {
            await _blobContainerClient.CreateIfNotExistsAsync();
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
            BlobHttpHeaders blobHttpHeader = mimeType != null
                ? new BlobHttpHeaders()
                {
                    ContentType = mimeType
                }
                : null;

            if (await blobClient.ExistsAsync())
            {
                if (blobHttpHeader != null)
                {
                    await blobClient.SetHttpHeadersAsync(blobHttpHeader);
                }

                await blobClient.UploadAsync(mediaBinaryStream, overwrite: true);
            }
            else
            {
                await blobClient.UploadAsync(mediaBinaryStream, blobHttpHeader);
            }
        }
    }
}