using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace EShop.Core.Catalog.Products.Price;

public readonly struct Money : IHtmlContent,  IEquatable<Money>, IComparable<Money>, IComparable
{
    private static readonly NumberFormatInfo NumberFormat = CultureInfo.CreateSpecificCulture("en-US")
        .NumberFormat;


    public Money(decimal amount, string postFormat = null)
    {
        Amount = amount;
        PostFormat = postFormat;
    }

    public decimal Amount { get; init; }
    public string PostFormat { get; init; }
    public int DecimalDigits => 2;

    internal decimal RoundedAmount
    {
        get => decimal.Round(Amount, DecimalDigits, MidpointRounding.AwayFromZero);
    }

    public Money WithPostFormat(string postFormat) => new(Amount, postFormat);
    public Money WithAmount(decimal amount) => new(amount, PostFormat);

    public override string ToString()
    {
        var formatted = RoundedAmount.ToString("C", NumberFormat);
        return PostFormat == null ? formatted : string.Format(PostFormat, formatted, CultureInfo.InvariantCulture);
    }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
         writer.Write(ToString());
    }


    #region Equality & Order

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount);
    }

    public override bool Equals(object obj)
    {
        if (obj is Money money)
        {
            return Equals(money);
        }

        return false;
    }

    public bool Equals(Money other)
    {
        return Amount == other.Amount;
    }

    public static bool operator ==(Money left, Money right) => left.Equals(right);
    public static bool operator !=(Money left, Money right) => !left.Equals(right);
    public static bool operator >(Money left, Money right) => left.Equals(right);
    public static bool operator <(Money left, Money right) => !left.Equals(right);

    public static bool operator ==(Money left, decimal right) => left.Amount == right;
    public static bool operator !=(Money left, decimal right) => left.Amount == right;

    public static bool operator >(Money left, decimal right) => left.Amount > right;
    public static bool operator <(Money left, decimal right) => left.Amount < right;

    public int CompareTo(object obj)
    {
        if (obj == null || obj is not Money other)
        {
            return 1;
        }

        return CompareTo(other);
    }

    public int CompareTo(Money other)
    {
        return Amount.CompareTo(other.Amount);
    }

    #endregion

    #region Arithmetics

    public static Money operator -(Money left, decimal right) => left.WithAmount(left.Amount - right);
    public static Money operator -(Money left, Money right) => left.WithAmount(left.Amount - right.Amount);
    public static Money operator /(Money left, Money right) => right.WithAmount(left.Amount / right.Amount);
    public static explicit operator float(Money money) => Convert.ToSingle(money.RoundedAmount);
    public static explicit operator decimal(Money money) => money.RoundedAmount;




    #endregion
}