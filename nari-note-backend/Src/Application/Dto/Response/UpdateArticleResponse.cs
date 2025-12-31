namespace NariNoteBackend.Application.Dto.Response;

public class UpdateArticleResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public bool IsPublished { get; set; }
    public DateTime UpdatedAt { get; set; }
}
