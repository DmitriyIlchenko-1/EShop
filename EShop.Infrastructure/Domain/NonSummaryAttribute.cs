namespace EShop.Infrastructure.Domain;

[AttributeUsage(AttributeTargets.Property)]
public class NonSummaryAttribute : Attribute
{
    public int? MaxLength { get; set; }

    public NonSummaryAttribute(int maxLength)   
    {
        if (maxLength < 1)
        {
            throw new ArgumentOutOfRangeException();
        }
        MaxLength = maxLength;
    }

    public NonSummaryAttribute()
    {
        
    }
}