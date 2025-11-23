using EShop.Infrastructure.Domain;

namespace EShop.Core.Content.Media.Domain;

public class Media : BaseEntity
{
    public string Filename { get; set; }

    public DateTime UploadedAtUtc { get; set; }
}