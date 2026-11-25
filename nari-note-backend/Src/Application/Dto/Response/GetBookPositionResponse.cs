using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetBookPositionResponse
{
    public BookId BookId { get; set; }
    public required string BookName { get; set; }
    public BookPositionId PositionId { get; set; }
    public required string Sfen { get; set; }
    public bool IsSenteTurn { get; set; }
    public string? LastMoveUsi { get; set; }
    public List<BookPathStepDto> Path { get; set; } = new();
    public List<string> LegalMoves { get; set; } = new();
    public List<BookMoveDto> Moves { get; set; } = new();
    public required ShapeGroupDto Group { get; set; }
    public List<ShapeProposalDto> Proposals { get; set; } = new();
    public required BookGraphDto Graph { get; set; }
    public int PendingJudgeCount { get; set; }
}
