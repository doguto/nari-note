using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;

namespace NariNoteBackend.Application.Helper;

// 読み込んだ定跡の局面と遷移を SFEN と指し手で引けるようにしたもの。局面・遷移・ノードの追加と削除はここで行い、Repository にも反映する
public class PlaybookGraph
{
    readonly Playbook playbook;
    readonly IPlaybookRepository repository;
    readonly Dictionary<string, PlaybookBoard> boardsBySfen;
    readonly Dictionary<PlaybookBoard, List<PlaybookBoardTransition>> outgoing = new();

    public PlaybookGraph(Playbook playbook, IPlaybookRepository repository)
    {
        this.playbook = playbook;
        this.repository = repository;
        this.boardsBySfen = playbook.Boards.ToDictionary(b => b.Sfen);
        foreach (var transition in playbook.Transitions.OrderBy(t => t.Id.Value))
        {
            this.OutgoingList(transition.FromBoard).Add(transition);
        }
    }

    public PlaybookBoard Root => this.boardsBySfen[this.playbook.RootSfen];

    public PlaybookBoard FindForceBoard(string sfen)
    {
        return this.boardsBySfen.GetValueOrDefault(sfen) ?? throw new KeyNotFoundException("この定跡に無い局面です");
    }

    public IReadOnlyList<PlaybookBoardTransition> Outgoing(PlaybookBoard board) =>
        this.outgoing.TryGetValue(board, out var list) ? list : [];

    // 開始局面からの手順を足す。指せない手が含まれていれば何も足さない
    public void AddLine(IReadOnlyList<string> usis)
    {
        var shogi = ShogiBoard.FromSfen(this.playbook.RootSfen);
        var played = new List<(string Usi, string To)>();
        foreach (var raw in usis)
        {
            var move = ShogiMove.Parse(raw);
            if (!shogi.IsLegal(move)) throw new ArgumentException($"この局面では指せない手です: {raw}");
            shogi = shogi.Apply(move);
            played.Add((move.ToUsi(), shogi.ToSfen()));
        }

        var from = this.Root;
        foreach (var (usi, toSfen) in played)
        {
            var existing = this.Outgoing(from).FirstOrDefault(t => t.Usi == usi);
            if (existing != null)
            {
                from = existing.ToBoard;
                continue;
            }

            var to = this.boardsBySfen.GetValueOrDefault(toSfen) ?? this.AddBoard(toSfen);
            var transition = new PlaybookBoardTransition { Playbook = this.playbook, FromBoard = from, ToBoard = to, Usi = usi };
            this.playbook.Transitions.Add(transition);
            this.OutgoingList(from).Add(transition);
            this.repository.AddTransition(transition);
            from = to;
        }
    }

    // 遷移を外し、開始局面から辿れなくなった局面と、そこから出る遷移も消す
    public void RemoveTransition(PlaybookBoardTransition transition)
    {
        this.DetachTransition(transition);

        var reachable = this.Reachable();
        foreach (var orphan in this.playbook.Transitions.Where(t => !reachable.Contains(t.FromBoard)).ToList())
        {
            this.DetachTransition(orphan);
        }
        foreach (var board in this.playbook.Boards.Where(b => !reachable.Contains(b)).ToList())
        {
            this.playbook.Boards.Remove(board);
            this.boardsBySfen.Remove(board.Sfen);
            this.repository.RemoveBoard(board);
            if (board.Node is { } node)
            {
                this.Unlink(board);
                this.ReleaseNodeIfEmpty(node);
            }
        }
    }

    public PlaybookNode EnsureNode(PlaybookBoard board)
    {
        if (board.Node != null) return board.Node;

        var node = new PlaybookNode { Playbook = this.playbook };
        node.Boards.Add(board);
        board.Node = node;
        this.playbook.Nodes.Add(node);
        this.repository.AddNode(node);
        return node;
    }

    public void Unlink(PlaybookBoard board)
    {
        board.Node?.Boards.Remove(board);
        board.Node = null;
        board.NodeId = null;
    }

    public void ReleaseNodeIfEmpty(PlaybookNode node)
    {
        if (node.Boards.Count > 0 && node.HasContent) return;

        foreach (var board in node.Boards.ToList()) this.Unlink(board);
        this.playbook.Nodes.Remove(node);
        this.repository.RemoveNode(node);
    }

    PlaybookBoard AddBoard(string sfen)
    {
        var board = new PlaybookBoard { Playbook = this.playbook, Sfen = sfen };
        this.playbook.Boards.Add(board);
        this.boardsBySfen[sfen] = board;
        this.repository.AddBoard(board);
        return board;
    }

    void DetachTransition(PlaybookBoardTransition transition)
    {
        this.OutgoingList(transition.FromBoard).Remove(transition);
        this.playbook.Transitions.Remove(transition);
        this.repository.RemoveTransition(transition);
    }

    List<PlaybookBoardTransition> OutgoingList(PlaybookBoard board)
    {
        if (!this.outgoing.TryGetValue(board, out var list))
        {
            list = new List<PlaybookBoardTransition>();
            this.outgoing[board] = list;
        }
        return list;
    }

    HashSet<PlaybookBoard> Reachable()
    {
        var seen = new HashSet<PlaybookBoard> { this.Root };
        var queue = new Queue<PlaybookBoard>(seen);
        while (queue.TryDequeue(out var board))
        {
            foreach (var transition in this.Outgoing(board))
            {
                if (seen.Add(transition.ToBoard)) queue.Enqueue(transition.ToBoard);
            }
        }
        return seen;
    }
}
