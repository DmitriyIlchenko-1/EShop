using EShop.Core.Content.Media.Domain;

namespace EShop.Core.Content.Media.Services;

public interface IMediaUrlHelper
{
    string GetUrl(MediaFile file);
}