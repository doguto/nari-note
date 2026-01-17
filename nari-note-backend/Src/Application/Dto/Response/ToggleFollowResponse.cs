namespace NariNoteBackend.Application.Dto.Response;

public class ToggleFollowResponse
{
    public bool IsFollowing { get; set; }
    public int CurrentFollowerCount { get; set; }
}
