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

// 研究盤で手元に指し進めた手順を、保存のときにまとめて定跡へ入れる
public class SavePlaybookLinesService
{
    readonly IPlaybookRepository playbookRepository;
    readonly TimeProvider timeProvider;

    public SavePlaybookLinesService(IPlaybookRepository playbookRepository, TimeProvider timeProvider)
    {
        this.playbookRepository = playbookRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<SavePlaybookLinesResponse> ExecuteAsync(UserId userId, SavePlaybookLinesRequest request)
    {
        var playbook = await this.playbookRepository.FindForceGraphByIdAsync(request.Id);
        PlaybookAccess.EnsureOwner(playbook, userId);

        var visited = new List<PlaybookPosition>();
        var changed = false;
        foreach (var line in request.Lines)
        {
            var position = PlaybookAccess.FindPosition(playbook, line.FromPositionId);
            foreach (var usi in line.Usis)
            {
                var (child, added) = this.Play(playbook, position, usi);
                changed |= added;
                visited.Add(child);
                position = child;
            }
        }

        if (changed)
        {
            playbook.UpdatedAt = this.timeProvider.UtcNow();
            await this.playbookRepository.SaveAsync();
        }

        return new SavePlaybookLinesResponse
        {
            Positions = visited.DistinctBy(p => p.Sfen).Select(p => new SavedPositionDto { Sfen = p.Sfen, PositionId = p.Id }).ToList(),
        };
    }

    (PlaybookPosition Child, bool Added) Play(Playbook playbook, PlaybookPosition parent, string usi)
    {
        var shogi = ShogiPosition.FromSfen(parent.Sfen);
        var move = ShogiMove.Parse(usi);
        if (!shogi.IsLegal(move)) throw new ArgumentException($"この局面では指せない手です: {usi}");
        var normalized = move.ToUsi();

        // 同じ保存の中で追加した手や局面はまだ Id を持たないので、参照で探す
        var existing = playbook.Moves.FirstOrDefault(m => m.ParentPosition == parent && m.Usi == normalized);
        if (existing != null) return (existing.ChildPosition, false);

        var childSfen = shogi.Apply(move).ToSfen();
        var child = playbook.Positions.FirstOrDefault(p => p.Sfen == childSfen);
        if (child == null)
        {
            var group = new ShapeGroup { Playbook = playbook };
            child = new PlaybookPosition { Playbook = playbook, Sfen = childSfen, ShapeGroup = group };
            this.playbookRepository.AddShapeGroup(group);
            this.playbookRepository.AddPosition(child);
            playbook.ShapeGroups.Add(group);
            playbook.Positions.Add(child);
        }

        var playbookMove = new PlaybookMove
        {
            Playbook = playbook,
            ParentPosition = parent,
            ChildPosition = child,
            Usi = normalized,
            Label = ShogiNotation.MoveLabel(shogi, move),
        };
        this.playbookRepository.AddMove(playbookMove);
        playbook.Moves.Add(playbookMove);
        return (child, true);
    }
}
