using EShop.Infrastructure.Domain;
using Microsoft.Extensions.FileProviders;

namespace EShop.Core.Content.Media.Domain;

/// <summary>
/// This type represents the metadata of a file.
/// </summary>
public class MediaFile : BaseEntity, IAuditableEntity, ISoftDeletableEntity
{
    public string FileName { get; set; }
    public string Alt { get; set; }
    public string MimeType { get; set; }
    public string MediaType { get; set; }
    public int Size { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime ModifiedOnUtc { get; set; }
    public bool Deleted { get; set; }
}