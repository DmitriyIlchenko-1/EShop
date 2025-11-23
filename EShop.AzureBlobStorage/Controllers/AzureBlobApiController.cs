using EShop.AzureBlobStorage.Configuration;
using EShop.Core.Platform.Configuration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EShop.AzureBlobStorage.Controllers;

[Authorize(Roles="Administrator")]
[Route("api/azureblobstorage")]
public class AzureBlobApiController : Controller
{
    private readonly ISettingFactory _settingFactory;

    public AzureBlobApiController(ISettingFactory settingFactory)
    {
        _settingFactory = settingFactory;
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettingsAsync()
    {
        var azureBlobSettings = await _settingFactory.LoadSettingsAsync<AzureBlobSettings>();
        // do the mapping
        throw new NotImplementedException();
    }
}