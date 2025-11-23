using EShop.Web.Common.Models;

namespace EShop.Web.Models.Widgets
{
    public class WidgetInstanceModel : BaseModel
    {
        public string Data { get; set; }

        public string HtmlData { get; set; }

        public long Id { get; set; }

        public string Name { get; set; }

        public string ViewComponentName { get; set; }

        public string WidgetId { get; set; }

        public long WidgetZoneId { get; set; }

         
    }
}