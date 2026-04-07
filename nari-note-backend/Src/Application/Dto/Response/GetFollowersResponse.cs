namespace NariNoteBackend.Application.Dto.Response;

public class GetFollowersResponse
{
    public List<FollowerUserDto> Followers { get; set; } = new();
}
