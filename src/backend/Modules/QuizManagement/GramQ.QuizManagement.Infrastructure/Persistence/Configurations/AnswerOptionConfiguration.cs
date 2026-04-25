using GramQ.QuizManagement.Domain.Aggregates.Quizzes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GramQ.QuizManagement.Infrastructure.Persistence.Configurations;

public class AnswerOptionConfiguration : IEntityTypeConfiguration<AnswerOption>
{
    public void Configure(EntityTypeBuilder<AnswerOption> builder)
    {
        builder.HasKey(ao => ao.Id);

        builder.HasIndex("QuestionId");

        builder.Property(ao => ao.Order);
        builder.Property(ao => ao.Text).HasMaxLength(QuizRules.MaxAnswerOptionTextLength).IsRequired();
        builder.Property(ao => ao.IsCorrect);
    }
}
