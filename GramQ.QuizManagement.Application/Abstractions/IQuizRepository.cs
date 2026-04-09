using GramQ.QuizManagement.Application.Queries;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes;
using GramQ.Shared.Abstractions.Pagination;

namespace GramQ.QuizManagement.Application.Abstractions;

public interface IQuizRepository
{
    Task<Quiz?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<Quiz>> GetPagedAsync(QuizFilter filter, CancellationToken cancellationToken);
    void Add(Quiz quiz);
}
