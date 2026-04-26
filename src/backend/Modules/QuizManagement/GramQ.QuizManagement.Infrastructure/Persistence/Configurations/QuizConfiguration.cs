using GramQ.QuizManagement.Domain.Aggregates.Quizzes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GramQ.QuizManagement.Infrastructure.Persistence.Configurations;

public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.HasKey(q => q.Id);
        builder.HasQueryFilter(q => !q.IsDeleted);

        builder.HasIndex(q => q.CreatedBy);

        builder.Navigation(q => q.Questions)
            .HasField("_questions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany<Question>(q => q.Questions)
            .WithOne()
            .HasForeignKey("QuizId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(q => q.Title).HasMaxLength(QuizRules.MaxQuizTitleLength).IsRequired();
        builder.Property(q => q.CreatedAt).IsRequired();
        builder.Property(q => q.CreatedBy).IsRequired();
        builder.Property(q => q.UpdatedAt);
        builder.Property(q => q.UpdatedBy);
        builder.Property(q => q.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(q => q.DeletedAt);

        builder.Property(q => q.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
    }
}
