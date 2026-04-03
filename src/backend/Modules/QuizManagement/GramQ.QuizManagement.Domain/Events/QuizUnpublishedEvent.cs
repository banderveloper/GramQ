using GramQ.Shared.Abstractions.Domain;

namespace GramQ.QuizManagement.Domain.Events;

public record QuizUnpublishedEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid QuizId,
    Guid UnpublishedBy) : IDomainEvent;
