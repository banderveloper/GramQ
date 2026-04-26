using GramQ.QuizManagement.Application.Queries;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes;

namespace GramQ.Api.Requests.Quiz;

public sealed record GetQuizzesRequest
{
    public QuizStatus? Status { get; init; }
    public string? TitleContains { get; init; }
    public QuizFilter.QuizSortBy? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
