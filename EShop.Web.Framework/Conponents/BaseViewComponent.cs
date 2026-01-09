using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EShop.Web.Common.Conponents;

public abstract class BaseViewComponent : ViewComponent
{
    private readonly static ContentViewComponentResult _emptyResult = new ContentViewComponentResult(string.Empty);

    protected ContentViewComponentResult NoContent() => _emptyResult;
}