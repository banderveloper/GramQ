using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Application.Queries;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes;
using GramQ.QuizManagement.Infrastructure.Persistence.DbContext;
using GramQ.Shared.Abstractions.Pagination;

using Microsoft.EntityFrameworkCore;

namespace GramQ.QuizManagement.Infrastructure.Persistence.Repositories;

public sealed class QuizRepository(QuizManagementDbContext dbContext) : IQuizRepository
{
    public async Task<Quiz?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Quizzes
            .Include("_questions")
            .Include("_questions._answerOptions")
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Quiz>> GetPagedAsync(QuizFilter filter, CancellationToken cancellationToken)
    {
        // todo include shadow prop?
        IQueryable<Quiz> query = dbContext.Quizzes;

        if (filter.CreatedBy != null)
            query = query.Where(q => q.CreatedBy == filter.CreatedBy);

        if (filter.Status != null)
            query = query.Where(q => q.Status == filter.Status);

        if (filter.TitleContains != null)
            query = query.Where(q => EF.Functions.ILike(q.Title, $"%{filter.TitleContains}%"));

        query = filter.SortBy switch
        {
            QuizFilter.QuizSortBy.CreatedAt => filter.SortDescending
                ? query.OrderByDescending(q => q.CreatedAt)
                : query.OrderBy(q => q.CreatedAt),
            QuizFilter.QuizSortBy.Title => filter.SortDescending
                ? query.OrderByDescending(q => q.Title)
                : query.OrderBy(q => q.Title),
            QuizFilter.QuizSortBy.Status => filter.SortDescending
                ? query.OrderByDescending(q => q.Status)
                : query.OrderBy(q => q.Status),
            _ => query.OrderBy(q => q.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var elements = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Quiz>
        {
            Items = elements,
            PageSize = filter.PageSize,
            Page = filter.Page,
            TotalCount = totalCount
        };
    }

    public void Add(Quiz quiz)
    {
        dbContext.Quizzes.Add(quiz);
    }
}
