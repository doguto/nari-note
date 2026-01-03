using System.Net;

namespace NariNoteBackend.Extension;

public static class HttpStatusCodeExtension
{
    public static int AsInt(this HttpStatusCode httpStatusCode)
    {
        return (int)httpStatusCode;
    }
}
