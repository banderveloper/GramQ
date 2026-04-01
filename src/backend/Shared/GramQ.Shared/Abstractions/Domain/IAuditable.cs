namespace GramQ.Shared.Abstractions.Domain;

public interface IAuditable
{
    DateTimeOffset CreatedAt{ get; set; }
    Guid? CreatedBy { get; set; }

    DateTimeOffset? UpdatedAt { get; set; }
    Guid? UpdatedBy { get; set; }
}
