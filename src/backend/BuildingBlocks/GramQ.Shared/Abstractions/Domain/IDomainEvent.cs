namespace GramQ.Shared.Abstractions.Domain;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredOn { get; }
}
