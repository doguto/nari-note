using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Domain.ValueObject;
using NSubstitute;

namespace NariNoteBackend.Tests.Support.Builder;

// Service の単体テスト用に、ノード・盤面・手順を持つ定跡をメモリ上に組み立てる。
// 手順の分け方は本番と同じ PlaybookTree と Rebuild に任せ、ID は連番で振る。DB に投入する結合テストでは使わない
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
        var node = new PlaybookNode { Playbook = this.playbook };
        var root = new PlaybookNodeBoard { Playbook = this.playbook, Node = node, Sfen = sfen };
        node.Boards.Add(root);
        this.playbook.Nodes.Add(node);
        this.playbook.Boards.Add(root);
        this.AssignIds();
    }

    public PlaybookNodeBoard Root => this.BoardAt(this.playbook.RootSfen)!;

    public Playbook Build() => this.playbook;

    public PlaybookNodeBoard? BoardAt(string sfen) => this.playbook.Boards.FirstOrDefault(b => b.Sfen == sfen);

    // 開始局面からの手順を足し、終点の局面の SFEN を返す
    public string Line(params string[] usis)
    {
        var tree = PlaybookTree.Load(this.playbook);
        tree.AddLine(usis);
        PlaybookAccess.Rebuild(this.playbook, tree, this.repository);
        this.AssignIds();
        return Play(this.playbook.RootSfen, usis);
    }

    // 局面に名前を付けてノードにする
    public PlaybookNodeBoard Name(string sfen, string name)
    {
        var board = this.BoardAt(sfen);
        if (board == null)
        {
            var node = new PlaybookNode { Playbook = this.playbook };
            board = new PlaybookNodeBoard { Playbook = this.playbook, Node = node, Sfen = sfen };
            node.Boards.Add(board);
            this.playbook.Nodes.Add(node);
            this.playbook.Boards.Add(board);
        }
        board.Node.Name = name;
        PlaybookAccess.Rebuild(this.playbook, PlaybookTree.Load(this.playbook), this.repository);
        this.AssignIds();
        return board;
    }

    public void Merge(PlaybookNodeBoard keep, PlaybookNodeBoard drop)
    {
        var dropNode = drop.Node;
        foreach (var board in dropNode.Boards.ToList())
        {
            board.Node = keep.Node;
            board.NodeId = keep.NodeId;
            keep.Node.Boards.Add(board);
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
        foreach (var board in this.playbook.Boards.Where(b => !b.Id.IsInitialized()))
        {
            board.Id = PlaybookNodeBoardId.From(NextGuid());
            board.PlaybookId = this.playbook.Id;
        }
        foreach (var board in this.playbook.Boards) board.NodeId = board.Node.Id;
        foreach (var step in this.playbook.Steps)
        {
            if (!step.Id.IsInitialized()) step.Id = PlaybookNodeStepId.From(Next());
            step.PlaybookId = this.playbook.Id;
            step.FromBoardId = step.FromBoard.Id;
            step.ToBoardId = step.ToBoard?.Id;
        }
    }

    static int Next() => Interlocked.Increment(ref sequence);

    static Guid NextGuid() => new(Next(), 0, 0, new byte[8]);
}
