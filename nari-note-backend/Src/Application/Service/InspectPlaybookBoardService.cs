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

        var graph = new PlaybookGraph(playbook, this.playbookRepository);
        var board = graph.FindForceBoard(ShogiBoard.Normalize(request.Sfen));
        var shogi = ShogiBoard.FromSfen(board.Sfen);
        var name = board.Node?.Name ?? "";
        var response = new InspectPlaybookBoardResponse
        {
            BoardId = board.Id,
            NameSuggestions = ShogiFeatures.NameSuggestions(shogi).Where(n => n != name).ToList(),
        };
        if (board.Node is not { } node) return response;

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
        response.Similar = FindSimilar(playbook, node, shogi);
        response.ConditionSuggestions = SuggestConditions(graph, board, node, shogi);
        return response;
    }

    static List<PlaybookSimilarBoardDto> FindSimilar(Playbook playbook, PlaybookNode node, ShogiBoard shogi)
    {
        return playbook.Boards
            .Where(b => b.Node != null && b.Node != node)
            .Select(b => (Board: b, Shogi: ShogiBoard.FromSfen(b.Sfen)))
            .Select(x => (x.Board, x.Shogi, Units: ShogiSimilarity.UnitCount(shogi, x.Shogi)))
            .Where(x => x.Units.HasValue)
            .OrderBy(x => x.Units)
            .ThenBy(x => x.Board.Id.Value)
            .Take(MaxSimilar)
            .Select(x => new PlaybookSimilarBoardDto
            {
                BoardId = x.Board.Id,
                NodeName = x.Board.Node!.Name,
                Diff = ShogiDiff.Describe(shogi, x.Shogi),
            })
            .ToList();
    }

    // 同じ手で別のノードへ進む局面との違いこそが条件なので、それを先に比べる
    static List<PlaybookConditionSuggestionDto> SuggestConditions(PlaybookGraph graph, PlaybookBoard board, PlaybookNode node, ShogiBoard shogi)
    {
        var siblings = node.Boards.Where(b => b != board).ToList();
        if (siblings.Count == 0) return new List<PlaybookConditionSuggestionDto>();

        return graph.Outgoing(board)
            .Select(transition =>
            {
                var target = EdgeEnd(graph, transition);
                var ordered = siblings
                    .OrderByDescending(s => graph.Outgoing(s).Any(o => o.Usi == transition.Usi && EdgeEnd(graph, o) != target))
                    .Select(s => ShogiBoard.FromSfen(s.Sfen));
                return new PlaybookConditionSuggestionDto
                {
                    TransitionId = transition.Id,
                    Suggestions = ShogiFeatures.ConditionSuggestions(shogi, ordered).Where(c => c != transition.Condition).ToList(),
                };
            })
            .ToList();
    }

    // 辺の行き先のノード。ノードに着かずに止まる辺は null
    static PlaybookNode? EdgeEnd(PlaybookGraph graph, PlaybookBoardTransition first)
    {
        var board = first.ToBoard;
        while (board.Node == null && graph.Outgoing(board).Count == 1)
        {
            board = graph.Outgoing(board)[0].ToBoard;
        }
        return board.Node;
    }
}
