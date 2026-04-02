namespace GramQ.Shared.Abstractions.Domain;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    protected Entity(Guid id) => Id = id;

    /// <summary>Required by EF Core. Do not use in application code.</summary>
    protected Entity()
    {
    }
}
