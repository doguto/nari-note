using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Request;

public class SplitPlaybookBoardRequest
{
    public required PlaybookId Id { get; set; }
    public required PlaybookNodeBoardId BoardId { get; set; }
}
