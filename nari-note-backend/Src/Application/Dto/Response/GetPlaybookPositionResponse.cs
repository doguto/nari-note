using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Dto.Response;

public class GetPlaybookPositionResponse
{
    public PlaybookId PlaybookId { get; set; }
    public required string PlaybookName { get; set; }
    public PlaybookPositionId PositionId { get; set; }
    public required string Sfen { get; set; }
    public bool IsSenteTurn { get; set; }
    public string? LastMoveUsi { get; set; }
    public List<PlaybookPathStepDto> Path { get; set; } = new();
    public List<string> LegalMoves { get; set; } = new();
    public List<PlaybookMoveDto> Moves { get; set; } = new();
    public required ShapeGroupDto Group { get; set; }
    public List<ShapeProposalDto> Proposals { get; set; } = new();
    public required PlaybookGraphDto Graph { get; set; }
    public int PendingJudgeCount { get; set; }
}
