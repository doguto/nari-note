namespace NariNoteBackend.Extension;

public static class StringExtension
{
    public static bool IsNullOrEmpty(this string? str)
    {
        if (str == null) return true;

        return string.IsNullOrEmpty(str);
    }
}
