using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Tests.Support.Builder;

// Service の単体テスト用に、局面・手・形グループを持つ定跡をメモリ上に組み立てる。
// ID は連番で振るため、DB に投入する結合テストでは使わない（結合テストは API で作る）
public class BookBuilder
{
    static int sequence;

    readonly Book book;

    public BookBuilder(User owner, string? rootSfen = null)
    {
        this.book = new Book
        {
            Id = BookId.From(Next()),
            UserId = owner.Id,
            Name = "テスト定跡",
            RootSfen = ShogiPosition.Normalize(rootSfen ?? ShogiPosition.Hirate),
            UpdatedAt = TestTimeProvider.DefaultUtcNow.AddDays(-1),
        };
        this.Root = this.AddPosition(this.book.RootSfen);
    }

    public BookPosition Root { get; }

    public Book Build() => this.book;

    public BookPosition AddPosition(string sfen)
    {
        var group = new ShapeGroup { Id = ShapeGroupId.From(Next()), BookId = this.book.Id, Book = this.book };
        var position = new BookPosition
        {
            Id = BookPositionId.From(Next()),
            BookId = this.book.Id,
            Book = this.book,
            Sfen = ShogiPosition.Normalize(sfen),
            ShapeGroupId = group.Id,
            ShapeGroup = group,
            CandidatesScanned = true,
        };
        this.book.ShapeGroups.Add(group);
        this.book.Positions.Add(position);
        return position;
    }

    public BookPosition Play(BookPosition from, string usi)
    {
        var shogi = ShogiPosition.FromSfen(from.Sfen);
        var move = ShogiMove.Parse(usi);
        var childSfen = shogi.Apply(move).ToSfen();
        var child = this.book.Positions.FirstOrDefault(p => p.Sfen == childSfen) ?? this.AddPosition(childSfen);
        this.book.Moves.Add(new BookMove
        {
            Id = BookMoveId.From(Next()),
            BookId = this.book.Id,
            Book = this.book,
            ParentPositionId = from.Id,
            ParentPosition = from,
            ChildPositionId = child.Id,
            ChildPosition = child,
            Usi = usi,
            Label = ShogiNotation.MoveLabel(shogi, move),
        });
        return child;
    }

    public BookPosition Line(params string[] usis)
    {
        var position = this.Root;
        foreach (var usi in usis) position = this.Play(position, usi);
        return position;
    }

    public void Merge(BookPosition keep, BookPosition drop)
    {
        var dropGroup = drop.ShapeGroup;
        foreach (var p in this.book.Positions.Where(p => p.ShapeGroupId == dropGroup.Id).ToList())
        {
            p.ShapeGroupId = keep.ShapeGroupId;
            p.ShapeGroup = keep.ShapeGroup;
        }
        this.book.ShapeGroups.Remove(dropGroup);
    }

    public ShapeCandidate Candidate(BookPosition a, BookPosition b, string verdict = ShapeCandidate.VerdictSame, int? delta = 0)
    {
        var candidate = new ShapeCandidate
        {
            Id = ShapeCandidateId.From(Next()),
            BookId = this.book.Id,
            Book = this.book,
            PositionAId = a.Id,
            PositionA = a,
            PositionBId = b.Id,
            PositionB = b,
            Status = ShapeCandidateStatus.Done,
            Verdict = verdict,
            Delta = delta,
            ConfirmedDepth = 6,
        };
        this.book.ShapeCandidates.Add(candidate);
        return candidate;
    }

    static int Next() => Interlocked.Increment(ref sequence);
}
