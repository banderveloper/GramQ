using GramQ.QuizManagement.Domain.Aggregates.Quizzes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GramQ.QuizManagement.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");

        builder.HasKey(q => q.Id);

        builder.HasIndex("QuizId");

        builder.Property(q => q.Text).HasMaxLength(QuizRules.MaxQuestionTextLength).IsRequired();
        builder.Property(q => q.Order);
        builder.Property(q => q.Points);
        builder.Property(q => q.TimeLimitSeconds);

        builder.HasMany<AnswerOption>(q => q.AnswerOptions)
            .WithOne()
            .HasForeignKey("QuestionId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(q => q.AnswerOptions)
            .HasField("_answerOptions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
