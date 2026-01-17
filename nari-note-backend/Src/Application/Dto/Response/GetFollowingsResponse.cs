namespace NariNoteBackend.Application.Dto.Response;

public class GetFollowingsResponse
{
    public List<FollowerUserDto> Followings { get; set; } = new();
}
