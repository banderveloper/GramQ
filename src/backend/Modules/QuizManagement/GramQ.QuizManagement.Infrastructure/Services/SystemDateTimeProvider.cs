using GramQ.Shared.Abstractions.Time;

namespace GramQ.QuizManagement.Infrastructure.Services;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
