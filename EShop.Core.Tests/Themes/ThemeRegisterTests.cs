using System.Reflection;
using NUnit.Framework;
using EShop.Core.Platform.Themes;
using EShop.Infrastructure.FileSystem;
using EShop.Infrastructure.IO;
using Microsoft.Extensions.FileProviders;

namespace EShop.Core.Tests.Themes;

[TestFixture]
public class ThemeRegisterTests
{
    [Test]
    public void Test()
    {
        IEShopFileProvider provider = new DefaultFileProvider("/Users/dmitroilcenko/Desktop/EShop/EShop.Core.Tests/");
        DefaultThemeRegistry r = new DefaultThemeRegistry(provider);
        Assert.True(true);
    }
}