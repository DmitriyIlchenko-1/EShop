using System.Diagnostics;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Platform.Configuration.Domain;

[DebuggerDisplay("{Name}:{Value}")]
public class Setting : BaseEntity
{
    public string Name { get; set; }
    public string Value { get; set; }
}