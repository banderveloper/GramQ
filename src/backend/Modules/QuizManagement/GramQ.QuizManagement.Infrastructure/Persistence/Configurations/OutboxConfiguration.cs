using GramQ.QuizManagement.Infrastructure.Outbox;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GramQ.QuizManagement.Infrastructure.Persistence.Configurations;

public class OutboxConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Type).HasMaxLength(256).IsRequired();
        builder.Property(o => o.Payload)
            .HasColumnType("text")
            .IsRequired();
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.ProcessedAt);
    }
}
