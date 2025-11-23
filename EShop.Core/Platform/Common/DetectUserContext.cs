using EShop.Core.Data;
using EShop.Core.Platform.Identity.Services;
using EShop.Core.Platform.Web;
using Microsoft.AspNetCore.Http;

namespace EShop.Core.Platform.Common;

public class DetectUserContext
{
    public HttpContext? HttpContext { get; set; }
    public IWebHelper WebHelper { get; set; }
    public ApplicationDbContext Db { get; set; }
    public IUserService UserService { get; set; }
    public string ClientIdentity { get; set; }
    public bool? DenyGuest { get; set; }
    public bool? DenyBot { get; set; }
    public Guid? UserGuid { get; set; }
}