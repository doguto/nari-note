using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class GetPlaybookPositionService
{
    const string RootLabel = "開始";

    readonly IPlaybookRepository playbookRepository;
    readonly IShogiEngineGateway shogiEngineGateway;

    public GetPlaybookPositionService(IPlaybookRepository playbookRepository, IShogiEngineGateway shogiEngineGateway)
    {
        this.playbookRepository = playbookRepository;
        this.shogiEngineGateway = shogiEngineGateway;
    }

    public async Task<GetPlaybookPositionResponse> ExecuteAsync(UserId userId, GetPlaybookPositionRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);
        var current = PlaybookAccess.FindPosition(playbook, request.PositionId);
        var root = PlaybookAccess.Root(playbook);
        var shogi = ShogiPosition.FromSfen(current.Sfen);

        var movesByParent = playbook.Moves.OrderBy(m => m.Id.Value).ToLookup(m => m.ParentPositionId);
        var path = FindPath(root, current, movesByParent, playbook);
        var pathSteps = BuildPathSteps(path, playbook);
        int? previousTo = path.Count > 0 ? ShogiMove.Parse(path[^1].Usi).To : null;

        var groups = playbook.ShapeGroups.ToDictionary(g => g.Id);
        var group = groups[current.ShapeGroupId];
        var groupOf = playbook.Positions.ToDictionary(p => p.Id, p => p.ShapeGroupId);
        var siblings = playbook.Positions
                           .Where(p => p.ShapeGroupId == group.Id && p.Id != current.Id)
                           .OrderBy(p => p.Id.Value)
                           .Select(p => (p.Id, Shogi: ShogiPosition.FromSfen(p.Sfen)))
                           .ToList();

        var moves = movesByParent[current.Id]
            .Select(m => new PlaybookMoveDto
            {
                Id = m.Id,
                Usi = m.Usi,
                Label = ShogiNotation.MoveLabel(shogi, ShogiMove.Parse(m.Usi), previousTo),
                Memo = m.Memo,
                Condition = m.Condition,
                ConditionSuggestions = ShogiFeatures
                    .ConditionSuggestions(shogi, OrderByDivergence(m, siblings, movesByParent, groupOf))
                    .Where(c => c != m.Condition)
                    .ToList(),
                ChildPositionId = m.ChildPositionId,
            })
            .ToList();

        var representatives = PlaybookAccess.Representatives(playbook.Positions);
        var members = playbook.Positions
                          .Where(p => p.ShapeGroupId == group.Id)
                          .OrderBy(p => p.Id.Value)
                          .Select(p => new ShapeMemberDto
                          {
                              PositionId = p.Id,
                              IsCurrent = p.Id == current.Id,
                              IsRepresentative = representatives[group.Id].Id == p.Id,
                              Diff = ShogiDiff.Describe(shogi, ShogiPosition.FromSfen(p.Sfen)),
                          })
                          .ToList();

        var positionsById = playbook.Positions.ToDictionary(p => p.Id);
        var proposals = new List<ShapeProposalDto>();
        foreach (var c in playbook.ShapeCandidates.Where(c => c.IsProposal).OrderBy(c => c.Id.Value))
        {
            var a = positionsById[c.PositionAId];
            var b = positionsById[c.PositionBId];
            var aInGroup = a.ShapeGroupId == group.Id;
            if (aInGroup == (b.ShapeGroupId == group.Id)) continue;
            var other = aInGroup ? b : a;
            proposals.Add(new ShapeProposalDto
            {
                CandidateId = c.Id,
                OtherPositionId = other.Id,
                OtherName = groups[other.ShapeGroupId].Name,
                Verdict = c.Verdict!,
                Delta = c.Delta is { } d ? (aInGroup ? d : -d) : null,
                ConfirmedDepth = c.ConfirmedDepth,
                Diff = ShogiDiff.Describe(shogi, ShogiPosition.FromSfen(other.Sfen)),
            });
        }

        return new GetPlaybookPositionResponse
        {
            PlaybookId = playbook.Id,
            PlaybookName = playbook.Name,
            PositionId = current.Id,
            Sfen = current.Sfen,
            IsSenteTurn = shogi.Turn == Side.Sente,
            LastMoveUsi = path.Count > 0 ? path[^1].Usi : null,
            Path = pathSteps,
            LegalMoves = shogi.LegalMoves().Select(m => m.ToUsi()).ToList(),
            Moves = moves,
            Group = new ShapeGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Memo = group.Memo,
                NameSuggestions = ShogiFeatures.NameSuggestions(shogi).Where(n => n != group.Name).ToList(),
                Members = members,
            },
            Proposals = proposals,
            Graph = BuildGraph(playbook, root, current, representatives),
            // エンジンが無い環境では判定が進まないので、待ちがあるように見せない
            PendingJudgeCount = this.shogiEngineGateway.IsEnabled ? await this.playbookRepository.CountPendingJudgesAsync(playbook.Id) : 0,
        };
    }

    // 同じ手で別の形へ進む局面との違いこそが条件なので、それを先に並べる
    static IEnumerable<ShogiPosition> OrderByDivergence(
        PlaybookMove move,
        List<(PlaybookPositionId Id, ShogiPosition Shogi)> siblings,
        ILookup<PlaybookPositionId, PlaybookMove> movesByParent,
        Dictionary<PlaybookPositionId, ShapeGroupId> groupOf
    )
    {
        var childGroup = groupOf[move.ChildPositionId];
        return siblings
            .OrderByDescending(s => movesByParent[s.Id].Any(m => m.Usi == move.Usi && groupOf[m.ChildPositionId] != childGroup))
            .Select(s => s.Shogi);
    }

    static List<PlaybookMove> FindPath(PlaybookPosition root, PlaybookPosition target, ILookup<PlaybookPositionId, PlaybookMove> movesByParent, Playbook playbook)
    {
        var previous = new Dictionary<PlaybookPositionId, PlaybookMove?> { [root.Id] = null };
        var queue = new Queue<PlaybookPositionId>([root.Id]);
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            if (id == target.Id) break;
            foreach (var move in movesByParent[id])
            {
                if (previous.ContainsKey(move.ChildPositionId)) continue;
                previous[move.ChildPositionId] = move;
                queue.Enqueue(move.ChildPositionId);
            }
        }

        var path = new List<PlaybookMove>();
        if (!previous.ContainsKey(target.Id)) return path;
        var cursor = target.Id;
        while (previous[cursor] is { } step)
        {
            path.Add(step);
            cursor = step.ParentPositionId;
        }
        path.Reverse();
        return path;
    }

    static List<PlaybookPathStepDto> BuildPathSteps(List<PlaybookMove> path, Playbook playbook)
    {
        if (path.Count == 0) return new List<PlaybookPathStepDto>();
        var positionsById = playbook.Positions.ToDictionary(p => p.Id);
        var labels = ShogiNotation.SequenceLabels(positionsById[path[0].ParentPositionId].Sfen, path.Select(m => m.Usi));
        return path.Select((m, i) => new PlaybookPathStepDto
        {
            ParentPositionId = m.ParentPositionId,
            PositionId = m.ChildPositionId,
            Usi = m.Usi,
            Label = i < labels.Count ? labels[i] : m.Label,
        }).ToList();
    }

    static PlaybookGraphDto BuildGraph(
        Playbook playbook,
        PlaybookPosition root,
        PlaybookPosition current,
        Dictionary<ShapeGroupId, PlaybookPosition> representatives
    )
    {
        var groupOf = playbook.Positions.ToDictionary(p => p.Id, p => p.ShapeGroupId);
        var moves = playbook.Moves.OrderBy(m => m.Id.Value).ToList();
        var parentsWithMoves = moves.Select(m => groupOf[m.ParentPositionId]).ToHashSet();

        var edges = moves
            .GroupBy(m => (From: groupOf[m.ParentPositionId], m.Usi, To: groupOf[m.ChildPositionId]))
            .Select(g => (g.Key.From, g.Key.Usi, g.Key.To, g.First().Label, Condition: g.Select(m => m.Condition).FirstOrDefault(c => c.Length > 0) ?? ""))
            .ToList();
        var targetsByMove = edges.GroupBy(e => (e.From, e.Usi)).ToDictionary(g => g.Key, g => g.Count());

        var incomingLabel = new Dictionary<ShapeGroupId, string>();
        foreach (var m in moves)
        {
            incomingLabel.TryAdd(groupOf[m.ChildPositionId], m.Label);
        }

        var nodes = playbook.ShapeGroups
            .OrderBy(g => representatives[g.Id].Id.Value)
            .Select(g => new PlaybookGraphNodeDto
            {
                GroupId = g.Id,
                RepresentativePositionId = representatives[g.Id].Id,
                MoveLabel = g.Id == root.ShapeGroupId ? RootLabel : incomingLabel.GetValueOrDefault(g.Id, ""),
                Name = g.Name,
                PositionCount = playbook.Positions.Count(p => p.ShapeGroupId == g.Id),
                IsLeaf = !parentsWithMoves.Contains(g.Id),
                HasMemo = g.Memo.Length > 0,
            })
            .ToList();

        return new PlaybookGraphDto
        {
            RootGroupId = root.ShapeGroupId,
            CurrentGroupId = current.ShapeGroupId,
            Nodes = nodes,
            Edges = edges.Select(e => new PlaybookGraphEdgeDto
            {
                FromGroupId = e.From,
                ToGroupId = e.To,
                Label = e.Label,
                Condition = e.Condition,
                // 同じ形から同じ手を指して行き先が分かれるのは、まとめた局面の違いで手の意味が変わるところ
                IsConditional = targetsByMove[(e.From, e.Usi)] > 1,
            }).ToList(),
        };
    }
}
