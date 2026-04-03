using GramQ.Shared.Abstractions.Domain;

namespace GramQ.QuizManagement.Domain.Events;

public record QuizPublishedEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    Guid QuizId,
    Guid PublishedBy) : IDomainEvent;
