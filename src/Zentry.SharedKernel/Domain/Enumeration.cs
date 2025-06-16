using System.Reflection;

namespace Zentry.SharedKernel.Domain;

public abstract class Enumeration(int id, string name) : IComparable
{
    private int Id { get; } = id;
    private string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    public int CompareTo(object? obj)
    {
        if (obj is Enumeration other) return Id.CompareTo(other.Id);
        throw new ArgumentException("Object is not an Enumeration.");
    }

    public override string ToString()
    {
        return Name;
    }

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        return typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>();
    }

    public override bool Equals(object? obj)
    {
        if (obj is Enumeration other) return Id == other.Id && Name == other.Name;
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name);
    }
}