using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Application.Queries;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes;
using GramQ.QuizManagement.Infrastructure.Persistence.DbContext;
using GramQ.Shared.Abstractions.Pagination;

using Microsoft.EntityFrameworkCore;

namespace GramQ.QuizManagement.Infrastructure.Persistence.Repositories;

public sealed class QuizRepository(QuizManagementDbContext dbContext, IUnitOfWork uow) : IQuizRepository
{
    public async Task<Quiz?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        // todo include shadow prop?
        return await dbContext.Quizzes.FindAsync([id], cancellationToken);
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
            query = query.Where(q => q.Title.ToLower().Contains(filter.TitleContains.ToLower()));

        if (filter.SortBy != null)
        {
            query = filter.SortBy switch
            {
                // todo add sort
                _ => filter.SortDescending ? query.OrderByDescending(q => q.Id) : query.OrderBy(q => q.Id)
            };
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var elements = await query
            .Skip((filter.PageSize - 1) * filter.Page)
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
