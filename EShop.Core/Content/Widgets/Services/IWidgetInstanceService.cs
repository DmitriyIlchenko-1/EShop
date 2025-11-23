using EShop.Core.Content.Widgets.Domain;

namespace EShop.Core.Content.Widgets.Services
{
	public interface IWidgetInstanceService
	{
		IQueryable<WidgetInstance> GetPublishedWidgetQuery();
	}
}