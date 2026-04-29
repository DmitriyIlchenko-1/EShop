using Microsoft.AspNetCore.Http;

namespace EShop.Infrastructure.Common;

/*
 * We use extension methods rather than define methods on this interface because of the ISP:
 * 1. If we were to, modules that don't use a subset of the methods defined on the interface would have to be recompiled
 * because of changes imposed by other clients on this interface. Those modules would also have to reference all the type they didn't use.
 * 2. We don't create multiple interfaces as one of the ISP suggestions because of the SRP.
 * Considering other way of designing this whole thing could also result in needless repetition.
 */
public interface IViewHelper
{
    public HttpContext HttpContext { get; }
}