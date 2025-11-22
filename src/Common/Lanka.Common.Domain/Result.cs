using System.Diagnostics.CodeAnalysis;

namespace Lanka.Common.Domain;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        this.IsSuccess = isSuccess;
        this.Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !this.IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    protected static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    public static implicit operator Result(Error error) => Failure(error);
}

public class Result<TValue> : Result
{
    public Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        this.Value = value;
    }

    [NotNull]
    public TValue Value =>
        this.IsSuccess
            ? field!
            : throw new InvalidOperationException(
                "The value of a failure result can't be accessed."
            );

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    public static Result<TValue> ValidationFailure(Error error) => new(default, false, error);
}
