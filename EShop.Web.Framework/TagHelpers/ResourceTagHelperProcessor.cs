using Microsoft.Extensions.Logging;
using OrchardCore.ResourceManagement;

namespace EShop.Web;

internal class ResourcesTagHelperProcessor : IResourcesTagHelperProcessor
{
    private readonly IResourceManager _resourceManager;
    private readonly ILogger<ResourcesTagHelperProcessor> _logger;

    public ResourcesTagHelperProcessor(
        IResourceManager resourceManager,
        ILogger<ResourcesTagHelperProcessor> logger)
    {
        this._resourceManager = resourceManager;
        this._logger = logger;
    }

    public Task ProcessAsync(ResourcesTagHelperProcessorContext context)
    {
        switch (context.Type)
        {
            case ResourceTagType.Meta:
                this._resourceManager.RenderMeta(context.Writer);
                break;
            case ResourceTagType.HeadLink:
                this._resourceManager.RenderHeadLink(context.Writer);
                break;
            case ResourceTagType.Stylesheet:
                this._resourceManager.RenderStylesheet(context.Writer);
                break;
            case ResourceTagType.HeadScript:
                this._resourceManager.RenderHeadScript(context.Writer);
                break;
            case ResourceTagType.FootScript:
                this._resourceManager.RenderFootScript(context.Writer);
                break;
            case ResourceTagType.Header:
                this._resourceManager.RenderMeta(context.Writer);
                this._resourceManager.RenderHeadLink(context.Writer);
                this._resourceManager.RenderStylesheet(context.Writer);
                this._resourceManager.RenderHeadScript(context.Writer);
                break;
            case ResourceTagType.Footer:
                this._resourceManager.RenderFootScript(context.Writer);
                break;
            default:
                this._logger.LogWarning("Unknown {TypeName} value \"{Value}\".", (object) "ResourceTagType", (object) context.Type);
                break;
        }
        return Task.CompletedTask;
    }
}
