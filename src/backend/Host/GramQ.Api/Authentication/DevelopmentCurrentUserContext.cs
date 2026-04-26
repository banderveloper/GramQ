using GramQ.QuizManagement.Application.Abstractions;

namespace GramQ.Api.Authentication;

public class DevelopmentCurrentUserContext : ICurrentUserContext
{
    public Guid UserId => Guid.Parse("00000000-0000-0000-0000-000000000001");
    public bool IsAdmin => true;
}
