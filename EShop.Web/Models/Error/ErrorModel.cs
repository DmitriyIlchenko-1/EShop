using System.Net;

namespace EShop.Web.Models.Error;

public class ErrorModel
{
    public string? Path { get; set; }
    public Exception? Exception { get; set; }
    public HttpStatusCode Status { get; set; }
}