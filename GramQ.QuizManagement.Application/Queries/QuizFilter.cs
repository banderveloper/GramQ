using GramQ.QuizManagement.Domain.Aggregates.Quizzes;

namespace GramQ.QuizManagement.Application.Queries;

public sealed record QuizFilter
{
    public Guid? CreatedBy { get; init; }
    public QuizStatus? Status { get; init; }
    public string? TitleContains { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
};
