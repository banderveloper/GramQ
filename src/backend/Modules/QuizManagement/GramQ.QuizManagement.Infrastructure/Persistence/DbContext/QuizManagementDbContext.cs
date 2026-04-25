using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes;
using GramQ.QuizManagement.Infrastructure.Outbox;

using Microsoft.EntityFrameworkCore;

namespace GramQ.QuizManagement.Infrastructure.Persistence.DbContext;

public class QuizManagementDbContext(DbContextOptions<QuizManagementDbContext> options)
    : Microsoft.EntityFrameworkCore.DbContext(options), IUnitOfWork
{
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        base.SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuizManagementDbContext).Assembly);
}
