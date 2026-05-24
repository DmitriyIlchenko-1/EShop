namespace EShop.Infrastructure.Media.Images;

public class ImageDescriptor
{
    public int Id { get; set; }
    public int MaxWidth { get; set; }
    public int MaxHeight { get; set; }
    public string Path { get; set; }
    public string Extension { get; set; }
}