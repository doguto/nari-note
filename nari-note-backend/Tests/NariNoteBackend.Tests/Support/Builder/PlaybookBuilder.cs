using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Domain.ValueObject;
using NSubstitute;

namespace NariNoteBackend.Tests.Support.Builder;

// Service の単体テスト用に、ノード・局面・遷移を持つ定跡をメモリ上に組み立てる。
// 局面と遷移の足し方は本番と同じ PlaybookGraph に任せ、ID は連番で振る。DB に投入する結合テストでは使わない
public class PlaybookBuilder
{
    static int sequence;

    readonly Playbook playbook;
    readonly IPlaybookRepository repository = Substitute.For<IPlaybookRepository>();

    public PlaybookBuilder(User owner, string? rootSfen = null)
    {
        var sfen = ShogiBoard.Normalize(rootSfen ?? ShogiBoard.Hirate);
        this.playbook = new Playbook
        {
            Id = PlaybookId.From(NextGuid()),
            UserId = owner.Id,
            Name = "テスト定跡",
            RootSfen = sfen,
            UpdatedAt = TestTimeProvider.DefaultUtcNow.AddDays(-1),
        };
        var root = new PlaybookBoard { Playbook = this.playbook, Sfen = sfen };
        this.playbook.Boards.Add(root);
        new PlaybookGraph(this.playbook, this.repository).EnsureNode(root);
        this.AssignIds();
    }

    public PlaybookBoard Root => this.BoardAt(this.playbook.RootSfen)!;

    public Playbook Build() => this.playbook;

    public PlaybookBoard? BoardAt(string sfen) => this.playbook.Boards.FirstOrDefault(b => b.Sfen == sfen);

    // 開始局面からの手順を足し、終点の局面の SFEN を返す
    public string Line(params string[] usis)
    {
        new PlaybookGraph(this.playbook, this.repository).AddLine(usis);
        this.AssignIds();
        return Play(this.playbook.RootSfen, usis);
    }

    public PlaybookBoard Name(string sfen, string name)
    {
        var board = this.BoardAt(sfen) ?? throw new InvalidOperationException("定跡に無い局面です");
        new PlaybookGraph(this.playbook, this.repository).EnsureNode(board).Name = name;
        this.AssignIds();
        return board;
    }

    public void Merge(PlaybookBoard keep, PlaybookBoard drop)
    {
        var dropNode = drop.Node!;
        foreach (var board in dropNode.Boards.ToList())
        {
            board.Node = keep.Node;
            board.NodeId = keep.NodeId;
            keep.Node!.Boards.Add(board);
        }
        this.playbook.Nodes.Remove(dropNode);
    }

    public static string Play(string sfen, params string[] usis)
    {
        var shogi = ShogiBoard.FromSfen(sfen);
        foreach (var usi in usis) shogi = shogi.Apply(ShogiMove.Parse(usi));
        return shogi.ToSfen();
    }

    void AssignIds()
    {
        foreach (var node in this.playbook.Nodes.Where(n => !n.Id.IsInitialized()))
        {
            node.Id = PlaybookNodeId.From(Next());
            node.PlaybookId = this.playbook.Id;
        }
        foreach (var board in this.playbook.Boards)
        {
            if (!board.Id.IsInitialized()) board.Id = PlaybookBoardId.From(NextGuid());
            board.PlaybookId = this.playbook.Id;
            board.NodeId = board.Node?.Id;
        }
        foreach (var transition in this.playbook.Transitions)
        {
            if (!transition.Id.IsInitialized()) transition.Id = PlaybookBoardTransitionId.From(Next());
            transition.PlaybookId = this.playbook.Id;
            transition.FromBoardId = transition.FromBoard.Id;
            transition.ToBoardId = transition.ToBoard.Id;
        }
    }

    static int Next() => Interlocked.Increment(ref sequence);

    static Guid NextGuid() => new(Next(), 0, 0, new byte[8]);
}
