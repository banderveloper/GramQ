namespace GramQ.Shared.Guards;

public static class Guard
{
    public static void ThrowIfDefault(Guid value, string paramName)
    {
        if(value == Guid.Empty)
            throw new ArgumentException("Value cannot be default (empty) Guid.", paramName);
    }
}
