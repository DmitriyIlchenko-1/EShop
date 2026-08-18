namespace EShop.Infrastructure.Email;

public class EmailException : Exception
{
    public EmailException(string message) : base(message)
    {
    }

    public EmailException(string message, Exception innerException) : base(message, innerException)
    {
    }
}