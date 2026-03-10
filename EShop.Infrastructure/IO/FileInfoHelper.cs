using EShop.Infrastructure.FileSystem;

namespace EShop.Infrastructure.IO;

internal static class FileInfoHelper
{
   public static bool IsExcluded(FileSystemInfo info)
   {
      
      if (info.Name.StartsWith(".", StringComparison.Ordinal))
      {
         return true;
      }
      else if (info.Exists && (info.Attributes & FileAttributes.Hidden) != 0 || (info.Attributes & FileAttributes.System) != 0)
      {
         return true;
      }

       
      return false;
   }
}