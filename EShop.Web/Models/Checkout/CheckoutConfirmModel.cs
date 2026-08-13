using EShop.Web.Common.Models;

namespace EShop.Web.Models.Checkout;

public class CheckoutConfirmModel : BaseModel
{
    public CheckoutDataSummaryModel SummaryModel { get; set; }
    public ICollection<string> Errors { get; set; } = new List<string>();
}