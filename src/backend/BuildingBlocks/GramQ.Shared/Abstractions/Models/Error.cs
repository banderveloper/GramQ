namespace GramQ.Shared.Abstractions.Models;



public readonly struct Error : IEquatable<Error>
{
    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Unexpected(string code, string description) =>
        new(code, description, ErrorType.Unexpected);

    public static Error Unauthorized(string code, string description) =>
        new(code, description, ErrorType.Unauthorized);

    public bool Equals(Error other) =>
        Code == other.Code && Type == other.Type;

    public override bool Equals(object? obj) =>
        obj is Error other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Code, Type);

    public static bool operator ==(Error left, Error right) => left.Equals(right);
    public static bool operator !=(Error left, Error right) => !left.Equals(right);

    public override string ToString() => $"{Type} | {Code}: {Description}";
}
