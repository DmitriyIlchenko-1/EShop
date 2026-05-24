// using System.Globalization;
// using Autofac;
// using EShop.Infrastructure.Engine;
// using EShop.Infrastructure.Extensions;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.FileProviders;
// using Microsoft.Extensions.Primitives;
//  
//
// namespace EShop.Infrastructure.Media.Images;
//
// public class FluidImageFileProvider : IFileProvider
// {
//     private readonly IFileProvider _imageFileProvider;
//     private readonly IImageProcessor _imageProcessor;
//
//     public FluidImageFileProvider(IFileProvider imageFileProvider, IImageProcessor imageProcessor)
//     {
//         _imageFileProvider = imageFileProvider;
//         _imageProcessor = imageProcessor;
//         
//     }
//
//     public IDirectoryContents GetDirectoryContents(string subpath)
//     {
//         throw new NotImplementedException();
//     }
//
//     public IFileInfo GetFileInfo(string subpath)
//     {
//         if (subpath.IsEmpty())
//         {
//             return new NotFoundFileInfo(subpath);
//         }
//
//         var scope  = EngineContext.Current.ChildLifetimeScopeAccessor.GetChildLifetimeScope;
//         var httpContext = scope.Resolve<IHttpContextAccessor>()?.HttpContext;
//         var cache = scope.Resolve<IImageCache>();
//         var imageDescriptor = new ImageDescriptor()
//         {
//             Path = subpath,
//             Width = httpContext.Request.Query.TryGetValue("w", out var width)
//                 ? (int.TryParse(width, CultureInfo.InvariantCulture, out int widthInt) ? widthInt : 0)
//                 : 0,
//             Height = httpContext.Request.Query.TryGetValue("h", out var height)
//                 ? (int.TryParse(height, CultureInfo.InvariantCulture, out int heightInt) ? heightInt : 0)
//                 : 0,
//         }; 
//       
//         var fileInfo = _imageFileProvider.GetFileInfo(imageDescriptor.Path);
//         var res = (_imageProcessor.ProcessImageAsync(new ProcessImageQuery()
//         {
//             Image = fileInfo,
//             ImageInfo = imageDescriptor
//         })).GetAwaiter().GetResult();
//          cache.PutAsync(res).GetAwaiter().GetResult();
//         return fileInfo;
//     }
//
//     private ImageDescriptor NormalizePath(string originalPath)
//     {
//         string path = originalPath;
//         var index = originalPath.IndexOf('?');
//
//         if (index != -1)
//         {
//             path = originalPath.Substring(0, index + 1);
//         }
//
//         var parameters =
//             GetParameters(originalPath.Substring(index, originalPath.Length - 1));
//
//
//         return new ImageDescriptor()
//         {
//             Path = path,
//             Width = 300,
//             Height = parameters.TryGetValue("height", out var height)
//                 ? (int.TryParse(height, CultureInfo.InvariantCulture, out int heightInt) ? heightInt : 0)
//                 : 0,
//         };
//
//         // media/pictures/main_logo.jpg?w=300;
//     }
//
//     private static Dictionary<string, string> GetParameters(string paramString)
//     {
//         return paramString
//             .TrimStart('?')
//             .Split(new[] { '&', ';' }, StringSplitOptions.RemoveEmptyEntries)
//             // {w=300, h=200}
//             .Select(x => x.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries))
//             // { {[w, 300]} {[h,200]} }
//             .GroupBy(pair => pair[0],
//                 pair
//                     => pair.Length > 2
//                         ? string.Join('=', pair, 1, pair.Length - 1)
//                         : (pair.Length > 1 ? pair[1] : string.Empty))
//             .ToDictionary(x => x.Key, x => string.Join(',', x));
//     }
//
//     public IChangeToken Watch(string filter)
//     {
//         throw new NotImplementedException();
//     }
//
//      
// }
//
// public class ImageDescriptor
// {
//     public int Width { get; set; }
//     public int Height { get; set; }
//     public string Path { get; set; }
// }