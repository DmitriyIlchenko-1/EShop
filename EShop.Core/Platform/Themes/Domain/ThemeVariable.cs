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

   
    public override bool Equals(BaseEntity other)
    {
        var equals =  base.Equals(other);
     
        if (!equals && other is ThemeVariable otherVariable)
        {
            return otherVariable.Theme.Equals(Theme, StringComparison.InvariantCulture)
                   && otherVariable.Name.Equals(Name, StringComparison.InvariantCulture);
        }
        return equals;
    }
}