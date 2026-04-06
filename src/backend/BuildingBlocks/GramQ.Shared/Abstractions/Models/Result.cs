namespace GramQ.Shared.Abstractions.Models;

public readonly struct Result
{
    private readonly Error? _error = null;
    private readonly bool _isSuccess = true;

    private Result(bool isSuccess)
    {
        _isSuccess = isSuccess;
        _error = null;
    }
    private Result(Error error)
    {
        _error = error;
        _isSuccess = false;
    }

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    public Error Error => !_isSuccess && _error.HasValue
        ? _error.Value
        : throw new InvalidOperationException(
            _isSuccess
                ? "Cannot access Error of a successful Result."
                : "Result is in invalid default state. Always use Result.Success() or Result.Failure().");

    public static Result Success() => new(true);
    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => Failure(error);
}
