namespace NariNoteBackend.Application.Dto.Response;

public class GetMyBooksResponse
{
    public List<BookSummaryDto> Books { get; set; } = new();
}
