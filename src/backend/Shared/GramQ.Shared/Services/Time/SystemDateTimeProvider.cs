using GramQ.Shared.Abstractions.Time;

namespace GramQ.Shared.Services.Time;

internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; set; }
}
