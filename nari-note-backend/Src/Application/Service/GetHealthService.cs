using System.Net;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class GetHealthService
{
    public Task<GetHealthResponse> ExecuteAsync()
    {
        return Task.FromResult(new GetHealthResponse()
        {
            StatusCode = HttpStatusCode.OK.AsInt(),
            Message = "",
        });
    }
}
