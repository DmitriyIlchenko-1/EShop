using System.ComponentModel.DataAnnotations;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Platform.Themes.Domain;

public class ThemeVariable : BaseEntity
{
    [StringLength(200)]
    public string Theme { get; set; }
    [StringLength(200)]
    public string Name { get; set; }
    [StringLength(1000)]
    public string Value { get; set; }

    //TODO: equality
    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
}