namespace NariNoteBackend.Application.Dto.Response;

public class CreateArticleResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}
