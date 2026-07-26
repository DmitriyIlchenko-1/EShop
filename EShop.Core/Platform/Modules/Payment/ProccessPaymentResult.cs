namespace EShop.Core.Platform.Modules.Payment;

public class ProcessPaymentResult(PaymentStatus paymentStatus)
{
    public PaymentStatus PaymentStatus { get; } = paymentStatus;
    public bool Succeeded => !Errors.Any();
    public IEnumerable<string> Errors { get; set; }
}