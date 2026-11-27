using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto;

public class PlaybookSimilarBoardDto
{
    public PlaybookBoardId BoardId { get; set; }
    public required string NodeName { get; set; }
    public required string Diff { get; set; }
}
