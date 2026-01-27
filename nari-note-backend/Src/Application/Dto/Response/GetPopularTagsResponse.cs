using NariNoteBackend.Application.Dto;

namespace NariNoteBackend.Application.Dto.Response;

public class GetPopularTagsResponse
{
    public List<TagDto> Tags { get; set; } = new();
}
