using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class InspectPlaybookBoardService
{
    const int MaxSimilar = 5;

    readonly IPlaybookRepository playbookRepository;

    public InspectPlaybookBoardService(IPlaybookRepository playbookRepository)
    {
        this.playbookRepository = playbookRepository;
    }

    public async Task<InspectPlaybookBoardResponse> ExecuteAsync(UserId userId, InspectPlaybookBoardRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var sfen = ShogiBoard.Normalize(request.Sfen);
        if (!PlaybookTree.Load(playbook).Contains(sfen)) throw new KeyNotFoundException("この定跡に無い局面です");
        var shogi = ShogiBoard.FromSfen(sfen);
        var board = playbook.Boards.FirstOrDefault(b => b.Sfen == sfen);
        var name = board?.Node.Name ?? "";
        var response = new InspectPlaybookBoardResponse
        {
            BoardId = board?.Id,
            NameSuggestions = ShogiFeatures.NameSuggestions(shogi).Where(n => n != name).ToList(),
        };
        if (board == null) return response;

        var node = board.Node;
        var representative = PlaybookAccess.Representative(node);
        response.Members = node.Boards
            .OrderBy(b => b.Id.Value)
            .Select(b => new PlaybookMemberDto
            {
                BoardId = b.Id,
                IsCurrent = b == board,
                IsRepresentative = b == representative,
                Diff = ShogiDiff.Describe(shogi, ShogiBoard.FromSfen(b.Sfen)),
            })
            .ToList();
        response.Similar = FindSimilar(playbook, board, shogi);
        response.ConditionSuggestions = SuggestConditions(playbook, board, shogi);
        return response;
    }

    static List<PlaybookSimilarBoardDto> FindSimilar(Playbook playbook, PlaybookNodeBoard board, ShogiBoard shogi)
    {
        return playbook.Boards
            .Where(b => b.Node != board.Node)
            .Select(b => (Board: b, Shogi: ShogiBoard.FromSfen(b.Sfen)))
            .Select(x => (x.Board, x.Shogi, Units: ShogiSimilarity.UnitCount(shogi, x.Shogi)))
            .Where(x => x.Units.HasValue)
            .OrderBy(x => x.Units)
            .ThenBy(x => x.Board.Id.Value)
            .Take(MaxSimilar)
            .Select(x => new PlaybookSimilarBoardDto
            {
                BoardId = x.Board.Id,
                NodeName = x.Board.Node.Name,
                Diff = ShogiDiff.Describe(shogi, x.Shogi),
            })
            .ToList();
    }

    // 同じ手で別のノードへ進む盤面との違いこそが条件なので、それを先に比べる
    static List<PlaybookConditionSuggestionDto> SuggestConditions(Playbook playbook, PlaybookNodeBoard board, ShogiBoard shogi)
    {
        var siblings = board.Node.Boards.Where(b => b != board).ToList();
        if (siblings.Count == 0) return new List<PlaybookConditionSuggestionDto>();

        var stepsByBoard = playbook.Steps.ToLookup(s => s.FromBoardId);
        return stepsByBoard[board.Id]
            .OrderBy(s => s.Id.Value)
            .Select(step =>
            {
                var target = step.ToBoard?.NodeId;
                var ordered = siblings
                    .OrderByDescending(s => stepsByBoard[s.Id].Any(o => o.Usis[0] == step.Usis[0] && o.ToBoard?.NodeId != target))
                    .Select(s => ShogiBoard.FromSfen(s.Sfen));
                return new PlaybookConditionSuggestionDto
                {
                    StepId = step.Id,
                    Suggestions = ShogiFeatures.ConditionSuggestions(shogi, ordered).Where(c => c != step.Condition).ToList(),
                };
            })
            .ToList();
    }
}
