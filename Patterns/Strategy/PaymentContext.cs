namespace EventBookingSystem.Patterns.Strategy;

/// <summary>
/// Strategy Pattern context: holds reference to chosen strategy and delegates execution.
/// </summary>
public class PaymentContext
{
    private IPaymentStrategy? _strategy;

    public void SetStrategy(IPaymentStrategy strategy) => _strategy = strategy;

    public PaymentResult Process(decimal amount, IDictionary<string, string> details)
    {
        if (_strategy == null)
            return new PaymentResult(false, "", "Payment method not selected.");
        return _strategy.Pay(amount, details);
    }

    public string CurrentMethod => _strategy?.MethodName ?? "None";

    public static IPaymentStrategy Resolve(string method) => method switch
    {
        "CreditCard" => new CreditCardStrategy(),
        "OnlineBanking" => new OnlineBankingStrategy(),
        "EWallet" => new EWalletStrategy(),
        _ => throw new ArgumentException($"Unsupported payment method: {method}")
    };
}
