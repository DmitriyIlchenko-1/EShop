namespace EShop.Core.Data.Cart.Exceptions;

public class ShoppingCartException : Exception
{
    public ShoppingCartException(string message) : base(message)
    {
    }

    public ShoppingCartException(string message, Exception innerException) : base(message, innerException)
    {
    }
}