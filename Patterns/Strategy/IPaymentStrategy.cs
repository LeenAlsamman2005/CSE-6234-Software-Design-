namespace EventBookingSystem.Patterns.Strategy;

public record PaymentResult(bool Success, string Reference, string Message);

/// <summary>
/// Strategy Pattern: payment algorithm interface.
/// New payment methods plug in without touching checkout logic.
/// </summary>
public interface IPaymentStrategy
{
    string MethodName { get; }
    PaymentResult Pay(decimal amount, IDictionary<string, string> details);
}
