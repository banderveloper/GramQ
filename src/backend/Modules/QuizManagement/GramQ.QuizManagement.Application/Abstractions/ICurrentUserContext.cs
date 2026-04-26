namespace GramQ.QuizManagement.Application.Abstractions;

public interface ICurrentUserContext
{
    Guid UserId { get; }
    bool IsAdmin { get; }
}
