namespace GramQ.QuizManagement.Infrastructure.Persistence.DbContext;

public sealed class QuizManagementDbOptions
{
    public const string SectionName = "QuizManagementDb";
    public required string ConnectionString { get; init; }
}
