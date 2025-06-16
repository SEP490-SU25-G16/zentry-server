using Zentry.SharedKernel.Abstractions.Domain;

namespace Zentry.SharedKernel.Domain;

public abstract class Entity(object id) : IEntity
{
    public object Id { get; } = id ?? throw new ArgumentNullException(nameof(id));

    public override bool Equals(object? obj)
    {
        if (obj is Entity other) return Equals(Id, other.Id);
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}