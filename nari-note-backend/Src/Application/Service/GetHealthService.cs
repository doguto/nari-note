using System.Net;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class GetHealthService
{
    public async Task<GetHealthResponse> ExecuteAsync()
    {
        return new GetHealthResponse()
        {
            StatusCode = HttpStatusCode.OK.AsInt(),
            Message = "",
        };
    }
}
