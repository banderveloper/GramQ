using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Application.Queries;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes;
using GramQ.Shared.Abstractions.Models;
using GramQ.Shared.Abstractions.Pagination;

namespace GramQ.QuizManagement.Application.UseCases.Queries;

public sealed record GetQuizzesQuery(QuizFilter Filter);

public sealed record GetQuizzesQueryResult
{
    public sealed record Quiz(
        Guid Id,
        string Title,
        QuizStatus Status,
        Guid CreatedBy,
        DateTimeOffset CreatedAt,
        Guid? UpdatedBy,
        DateTimeOffset? UpdatedAt);
}

public sealed class GetQuizzesQueryHandler(
    IQuizRepository quizRepository,
    ICurrentUserContext currentUser)
{
    public async Task<Result<PagedResult<GetQuizzesQueryResult.Quiz>>> HandleAsync(
        GetQuizzesQuery query,
        CancellationToken cancellationToken)
    {
        Guid? authorFilter = currentUser.IsAdmin ? null : currentUser.UserId;

        var pagedQuizzes =
            await quizRepository.GetPagedAsync(query.Filter with { CreatedBy = authorFilter }, cancellationToken);

        return new PagedResult<GetQuizzesQueryResult.Quiz>
        {
            Page = pagedQuizzes.Page,
            PageSize = pagedQuizzes.PageSize,
            TotalCount = pagedQuizzes.TotalCount,
            Items = pagedQuizzes.Items.Select(quizAgg => new GetQuizzesQueryResult.Quiz(
                Id: quizAgg.Id,
                Title: quizAgg.Title,
                Status: quizAgg.Status,
                CreatedBy: quizAgg.CreatedBy,
                CreatedAt: quizAgg.CreatedAt,
                UpdatedAt: quizAgg.UpdatedAt,
                UpdatedBy: quizAgg.UpdatedBy)).ToList()
        };
    }
}
