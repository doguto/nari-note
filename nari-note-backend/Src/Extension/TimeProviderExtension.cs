namespace NariNoteBackend.Extension;

public static class TimeProviderExtension
{
    public static DateTime UtcNow(this TimeProvider timeProvider)
    {
        return timeProvider.GetUtcNow().UtcDateTime;
    }
}
