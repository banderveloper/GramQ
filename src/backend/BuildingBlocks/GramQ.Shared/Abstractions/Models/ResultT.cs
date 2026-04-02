namespace GramQ.Shared.Abstractions.Models;

public readonly struct Result<TValue> where TValue : notnull
{
    private readonly TValue? _value;
    private readonly Error? _error;
    private readonly bool _isSuccess;

    private Result(TValue value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value),
                "Cannot create successful Result with null value.");
        _value = value;
        _isSuccess = true;
        _error = null;
    }

    private Result(Error error)
    {
        _error = error;
        _isSuccess = false;
        _value = default;
    }

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    public TValue Value => _isSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot access Value of a failed Result. Error: {_error}");

    public Error Error => !_isSuccess && _error.HasValue
        ? _error.Value
        : throw new InvalidOperationException(
            _isSuccess
                ? "Cannot access Error of a successful Result."
                : "Result is in invalid default state. Always use Result.Success() or Result.Failure().");

    public static Result<TValue> Success(TValue value) => new(value);
    public static Result<TValue> Failure(Error error) => new(error);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);

    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        _isSuccess ? onSuccess(_value!) : onFailure(Error);

    public static implicit operator Result<TValue>(Result result) =>
        result.IsSuccess
            ? throw new InvalidOperationException("Cannot convert successful Result to Result<TValue>.")
            : Failure(result.Error);
}
