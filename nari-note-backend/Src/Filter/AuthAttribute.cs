namespace NariNoteBackend.Filter;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthAttribute : Attribute
{
    public AuthAttribute(AuthLevel level)
    {
        this.Level = level;
    }

    public AuthLevel Level { get; }
}
