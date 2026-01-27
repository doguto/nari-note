namespace NariNoteBackend.Application.Dto.Response;

public class GetTagsResponse
{
    public List<TagDto> Tags { get; set; } = new();
}
