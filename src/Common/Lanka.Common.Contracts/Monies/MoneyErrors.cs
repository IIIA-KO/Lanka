using Lanka.Common.Domain;

namespace Lanka.Common.Contracts.Monies;

public static class MoneyErrors
{
    public static readonly Error NegativeAmount =
        Error.Validation("Money.NegativeAmount", "Money amount cannot be negative.");

    public static readonly Error DivisionByZero =
        Error.Validation("Money.DivisionByZero", "Cannot divide by zero.");

    public static Error CurrencyMismatch(string operation)
    {
        return Error.Validation("Money.CurrencyMismatch",
            $"Cannot {operation} amounts with different currencies.");
    }
}
