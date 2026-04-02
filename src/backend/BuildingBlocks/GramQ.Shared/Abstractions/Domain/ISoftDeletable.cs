namespace GramQ.Shared.Abstractions.Domain;

public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAt { get; }
}
