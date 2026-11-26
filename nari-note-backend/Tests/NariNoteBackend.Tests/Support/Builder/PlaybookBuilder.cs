using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Tests.Support.Builder;

// Service の単体テスト用に、局面・手・形グループを持つ定跡をメモリ上に組み立てる。
// ID は連番で振るため、DB に投入する結合テストでは使わない（結合テストは API で作る）
public class PlaybookBuilder
{
    static int sequence;

    readonly Playbook playbook;

    public PlaybookBuilder(User owner, string? rootSfen = null)
    {
        this.playbook = new Playbook
        {
            Id = PlaybookId.From(NextGuid()),
            UserId = owner.Id,
            Name = "テスト定跡",
            RootSfen = ShogiPosition.Normalize(rootSfen ?? ShogiPosition.Hirate),
            UpdatedAt = TestTimeProvider.DefaultUtcNow.AddDays(-1),
        };
        this.Root = this.AddPosition(this.playbook.RootSfen);
    }

    public PlaybookPosition Root { get; }

    public Playbook Build() => this.playbook;

    public PlaybookPosition AddPosition(string sfen)
    {
        var group = new ShapeGroup { Id = ShapeGroupId.From(Next()), PlaybookId = this.playbook.Id, Playbook = this.playbook };
        var position = new PlaybookPosition
        {
            Id = PlaybookPositionId.From(NextGuid()),
            PlaybookId = this.playbook.Id,
            Playbook = this.playbook,
            Sfen = ShogiPosition.Normalize(sfen),
            ShapeGroupId = group.Id,
            ShapeGroup = group,
            CandidatesScanned = true,
        };
        this.playbook.ShapeGroups.Add(group);
        this.playbook.Positions.Add(position);
        return position;
    }

    public PlaybookPosition Play(PlaybookPosition from, string usi)
    {
        var shogi = ShogiPosition.FromSfen(from.Sfen);
        var move = ShogiMove.Parse(usi);
        var childSfen = shogi.Apply(move).ToSfen();
        var child = this.playbook.Positions.FirstOrDefault(p => p.Sfen == childSfen) ?? this.AddPosition(childSfen);
        this.playbook.Moves.Add(new PlaybookMove
        {
            Id = PlaybookMoveId.From(Next()),
            PlaybookId = this.playbook.Id,
            Playbook = this.playbook,
            ParentPositionId = from.Id,
            ParentPosition = from,
            ChildPositionId = child.Id,
            ChildPosition = child,
            Usi = usi,
            Label = ShogiNotation.MoveLabel(shogi, move),
        });
        return child;
    }

    public PlaybookPosition Line(params string[] usis)
    {
        var position = this.Root;
        foreach (var usi in usis) position = this.Play(position, usi);
        return position;
    }

    public void Merge(PlaybookPosition keep, PlaybookPosition drop)
    {
        var dropGroup = drop.ShapeGroup;
        foreach (var p in this.playbook.Positions.Where(p => p.ShapeGroupId == dropGroup.Id).ToList())
        {
            p.ShapeGroupId = keep.ShapeGroupId;
            p.ShapeGroup = keep.ShapeGroup;
        }
        this.playbook.ShapeGroups.Remove(dropGroup);
    }

    public ShapeCandidate Candidate(PlaybookPosition a, PlaybookPosition b, string verdict = ShapeCandidate.VerdictSame, int? delta = 0)
    {
        var candidate = new ShapeCandidate
        {
            Id = ShapeCandidateId.From(Next()),
            PlaybookId = this.playbook.Id,
            Playbook = this.playbook,
            PositionAId = a.Id,
            PositionA = a,
            PositionBId = b.Id,
            PositionB = b,
            Status = ShapeCandidateStatus.Done,
            Verdict = verdict,
            Delta = delta,
            ConfirmedDepth = 6,
        };
        this.playbook.ShapeCandidates.Add(candidate);
        return candidate;
    }

    static int Next() => Interlocked.Increment(ref sequence);

    static Guid NextGuid() => new(Next(), 0, 0, new byte[8]);
}
