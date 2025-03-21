using Lanka.Common.Contracts.Currencies;
using Lanka.Common.Domain;

namespace Lanka.Common.Contracts.Monies;

public sealed record Money
{
    public decimal Amount { get; }

    public Currency Currency { get; }

    private Money() { }

    private Money(decimal amount, Currency currency)
    {
        this.Amount = amount;
        this.Currency = currency;
    }

    public static Result<Money> Create(decimal amount, Currency currency)
    {
        return amount < 0
            ? Result.Failure<Money>(MoneyErrors.NegativeAmount)
            : new Money(amount, currency);
    }

    public static Result<Money> operator +(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return left.Currency != right.Currency
            ? Result.Failure<Money>(MoneyErrors.CurrencyMismatch("add"))
            : Create(left.Amount + right.Amount, left.Currency);
    }

    public static Result<Money> operator -(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (left.Currency != right.Currency)
        {
            return Result.Failure<Money>(MoneyErrors.CurrencyMismatch("subtract"));
        }

        decimal resultAmount = left.Amount - right.Amount;
        return Create(resultAmount, left.Currency);
    }

    public static Result<Money> operator *(Money money, decimal multiplier)
    {
        ArgumentNullException.ThrowIfNull(money);

        return Create(money.Amount * multiplier, money.Currency);
    }

    public static Result<Money> operator /(Money money, decimal divisor)
    {
        ArgumentNullException.ThrowIfNull(money);

        return divisor == 0
            ? Result.Failure<Money>(MoneyErrors.DivisionByZero)
            : Create(money.Amount / divisor, money.Currency);
    }

    public static Result<decimal> operator /(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (left.Currency != right.Currency)
        {
            return Result.Failure<decimal>(MoneyErrors.CurrencyMismatch("divide"));
        }

        if (right.Amount == 0)
        {
            return Result.Failure<decimal>(MoneyErrors.DivisionByZero);
        }

        return left.Amount / right.Amount;
    }

    public static bool operator >(Money left, Money right)
    {
        ValidateComparisonOperands(left, right);
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        ValidateComparisonOperands(left, right);
        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        ValidateComparisonOperands(left, right);
        return left.Amount >= right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        ValidateComparisonOperands(left, right);
        return left.Amount <= right.Amount;
    }

    private static void ValidateComparisonOperands(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException(
                MoneyErrors.CurrencyMismatch("compare").Description);
        }
    }

    public static Money Zero(Currency currency)
    {
        return new Money(0, currency);
    }

    public bool IsZero()
    {
        return this.Amount == 0;
    }

    public override string ToString()
    {
        return $"{this.Amount} {this.Currency}";
    }

    public Result<Money> Add(Money other)
    {
        return this + other;
    }

    public Result<Money> Subtract(Money other)
    {
        return this - other;
    }

    public Result<Money> Multiply(decimal multiplier)
    {
        return this * multiplier;
    }

    public Result<Money> Divide(decimal divisor)
    {
        return this / divisor;
    }
}
