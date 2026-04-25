using System.Text.Json;
using System.Text.Json.Serialization;

using GramQ.QuizManagement.Infrastructure.Outbox;
using GramQ.Shared.Abstractions.Domain;
using GramQ.Shared.Abstractions.Time;

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GramQ.QuizManagement.Infrastructure.Persistence.Interceptors;

public class ConvertDomainEventsToOutboxMessagesInterceptor(
    IDateTimeProvider dateTimeProvider)
    : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions _options = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var aggregates = eventData.Context?.ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList() ?? [];

        foreach (var aggregate in aggregates)
        {
            foreach (var @event in aggregate.DomainEvents)
            {
                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = @event.GetType().FullName ?? @event.GetType().Name,
                    Payload = JsonSerializer.Serialize(@event, _options),
                    CreatedAt = dateTimeProvider.UtcNow
                };
                eventData.Context?.Add(outboxMessage);
            }

            aggregate.ClearDomainEvents();
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
