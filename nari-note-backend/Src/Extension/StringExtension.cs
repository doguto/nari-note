using System.Diagnostics.CodeAnalysis;

namespace NariNoteBackend.Extension;

public static class StringExtension
{
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str)
    {
        return string.IsNullOrEmpty(str);
    }
}
