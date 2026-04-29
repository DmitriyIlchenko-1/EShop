// using EShop.Infrastructure.Utilities;
// using Microsoft.Extensions.FileProviders;
// using Microsoft.Extensions.Primitives;
//
// namespace EShop.Infrastructure.IO;
//
// public class StaticContentFileProvider : IFileProvider
// {
//     private readonly Dictionary<string, IFileProvider> _fileProviders
//         = new Dictionary<string, IFileProvider>(StringComparer.InvariantCulture);
//
//
//     public void AddFileProvider(string scopePath, IFileProvider fileProvider)
//     {
//         Guard.NotEmpty(scopePath);
//         Guard.NotNull(fileProvider);
//         _fileProviders.TryAdd(scopePath, fileProvider);
//     }
//     
//     public void 
//
//     public IDirectoryContents GetDirectoryContents(string subpath)
//     {
//         throw new NotImplementedException();
//     }
//
//     public IFileInfo GetFileInfo(string subpath)
//     {
//         throw new NotImplementedException();
//     }
//
//     public IChangeToken Watch(string filter)
//     {
//         throw new NotImplementedException();
//     }
// }