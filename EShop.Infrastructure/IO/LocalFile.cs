namespace EShop.Infrastructure.IO;

public class LocalFile :  IFile
{
    private readonly FileInfo _fileInfo;

    public LocalFile(FileInfo fileInfo)
    {
        _fileInfo = fileInfo;
    }
}