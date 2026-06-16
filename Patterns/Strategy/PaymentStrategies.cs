namespace EventBookingSystem.Patterns.Strategy;

internal static class DictExt
{
    public static string Get(this IDictionary<string, string> d, string key) =>
        d.TryGetValue(key, out var v) ? v : "";
}

public class CreditCardStrategy : IPaymentStrategy
{
    public string MethodName => "Credit Card";

    public PaymentResult Pay(decimal amount, IDictionary<string, string> details)
    {
        var card = details.Get("CardNumber");
        if (string.IsNullOrWhiteSpace(card) || card.Replace(" ", "").Length < 12)
            return new PaymentResult(false, "", "Invalid card number.");
        var masked = "****-****-****-" + card[^4..];
        return new PaymentResult(true, $"CC-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            $"Paid RM{amount:F2} with Credit Card {masked}.");
    }
}

public class OnlineBankingStrategy : IPaymentStrategy
{
    public string MethodName => "Online Banking";

    public PaymentResult Pay(decimal amount, IDictionary<string, string> details)
    {
        var bank = details.Get("Bank");
        if (string.IsNullOrWhiteSpace(bank))
            return new PaymentResult(false, "", "Bank selection required.");
        return new PaymentResult(true, $"FPX-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            $"Paid RM{amount:F2} via Online Banking ({bank}).");
    }
}

public class EWalletStrategy : IPaymentStrategy
{
    public string MethodName => "E-Wallet";

    public PaymentResult Pay(decimal amount, IDictionary<string, string> details)
    {
        var wallet = details.Get("WalletId");
        if (string.IsNullOrWhiteSpace(wallet))
            return new PaymentResult(false, "", "Wallet ID required.");
        return new PaymentResult(true, $"EW-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            $"Paid RM{amount:F2} from E-Wallet {wallet}.");
    }
}
