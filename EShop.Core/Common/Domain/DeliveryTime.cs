using System.ComponentModel.DataAnnotations;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Common.Domain
{
    public class DeliveryTime : BaseEntity, IDisplayOrder
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
        [Required, StringLength(50)]
        public string ColorHexValue { get; set; }

        [Required, StringLength(50)]
        public string DisplayLocate { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsDefault { get; set; }

        public int? MaxDays { get; set; }

        public int? MinDays { get; set; }
    }
}