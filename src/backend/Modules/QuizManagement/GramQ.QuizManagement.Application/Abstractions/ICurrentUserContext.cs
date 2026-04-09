namespace GramQ.QuizManagement.Application.Abstractions;

public interface ICurrentUserContext
{
    Guid UserId { get; set; }
    bool IsAdmin { get; set; }
}
