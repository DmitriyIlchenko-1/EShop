using System.Reflection;
using System.Xml.Linq;
using NUnit.Framework;
using EShop.Core.Platform.Themes;
using EShop.Core.Platform.Themes.Services;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.FileSystem;
using EShop.Infrastructure.IO;
using EShop.Tests.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using Moq;
using Shouldly;

namespace EShop.Core.Tests.Themes;

[TestFixture]
public class ThemeRegisterTests
{
    private IThemeRegistry _themeRegistry;
    private string _themeFolderPath;
    private static ThemeDescriptor[] _themeDescriptors;

    [OneTimeSetUp]
    protected void Setup()
    {
        var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly()
            .Location);
        _themeFolderPath = Path.Join(currentLocation, "Themes");
        PrepareThemes();
        var envMock = new Mock<IWebHostEnvironment>();
        envMock
            .Setup(x => x.ContentRootPath)
            .Returns(currentLocation);
        var webEnv = envMock.Object;
        ILocalFileProvider fileProvider = new DefaultLocalFileProvider(webEnv.WebRootPath);
        _themeRegistry = new DefaultThemeRegistry(fileProvider);
    }

    private void PrepareThemes()
    {
        var theme1 = Path.Join(_themeFolderPath, "Theme1");
        var theme2 = Path.Join(_themeFolderPath, "Theme2");

        Directory.CreateDirectory(theme1);
        Directory.CreateDirectory(theme2);

        XDocument doc1 = XDocument.Parse(
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n<Themed name=\"Theme1\" description=\"Theme 1 description\" author=\"AuthorTheme1\" version=\"1.0\">\n    <Variables>\n        <Var name=\"white\" type=\"Color\">#99999</Var>\n    </Variables>\n</Themed>\n ");
        doc1.Save(Path.Join(theme1, "theme.config"));
        
        XDocument doc2 = XDocument.Parse(
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n<Themed name=\"Theme2\" description=\"Theme 2 description\" author=\"AuthorTheme2\" version=\"1.0\">\n    <Variables>\n        <Var name=\"white\" type=\"Color\">#99999</Var>\n    </Variables>\n</Themed>\n ");
        doc1.Save(Path.Join(theme2, "theme.config"));

        MaterializeThemes([doc1, doc2]);
    }

    private void MaterializeThemes(XDocument[] themes)
    {
        _themeDescriptors = new ThemeDescriptor[themes.Length];
        for (int i = 0; i < themes.Length; i++)
        {
            var root = themes[i].Root;
            var descriptor = new ThemeDescriptor();
            descriptor.ThemeName = ((string)root.Attribute("name")).EmptyIfNull();
            _themeDescriptors[i] = descriptor;
        }
    }


    [Test]
    public void Can_See_Theme()
    {
        string themeName = "Theme1";
        var result = _themeRegistry.Contains(themeName);
        result.ShouldBeTrue();
        string wrongThemeName = "NoTestTheme";
        result = _themeRegistry.Contains(wrongThemeName);
        result.ShouldBeFalse();
    }

    [Test]
    public void Can_Get_Theme_By_Name_Or_Null()
    {
        string themeName = "Theme1";
        var themeDescriptor = _themeRegistry.GetThemeByName(themeName);
        VerifyThemeMatch(themeDescriptor)
            .ShouldBeTrue();
        string wrongThemeName = "NoTestTheme";
        themeDescriptor = _themeRegistry.GetThemeByName(wrongThemeName);
        themeDescriptor.ShouldBeNull();
    }

    [Test]
    public void Can_Get_All_Themes()
    {
        var themes = _themeRegistry.GetThemeDescriptors();
        themes.ShouldNotBeEmpty();
        themes.Count.ShouldEqual(2);
        foreach (var theme in themes)
        {
            VerifyThemeMatch(theme)
                .ShouldBeTrue();
        }
    }

    private bool VerifyThemeMatch(ThemeDescriptor theme)
    {
        theme.ShouldNotBeNull();
        for (int i = 0; i < _themeDescriptors.Length; i++)
        {
            var themeDescriptor = _themeDescriptors[i];
            if (themeDescriptor.Equals(theme))
                return true;
        }
        
        return false;
    }
}