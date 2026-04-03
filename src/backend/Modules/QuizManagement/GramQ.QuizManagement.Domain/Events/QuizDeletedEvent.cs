using GramQ.Shared.Abstractions.Domain;

namespace GramQ.QuizManagement.Domain.Events;

public record QuizDeletedEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid QuizId,
    Guid DeletedBy) : IDomainEvent;
