using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;

namespace NariNoteBackend.Application.Helper;

// 読み込んだ定跡の局面と遷移を SFEN と指し手で引けるようにしたもの。局面・遷移・ノードの追加と削除はここで行い、Repository にも反映する。
// ノードでない局面は入る遷移も出る遷移も 1 本以下に保つので、ノードから次のノードまでの遷移の並びがグラフの辺 1 本になる
public class PlaybookGraph
{
    readonly Playbook playbook;
    readonly IPlaybookRepository repository;
    readonly Dictionary<string, PlaybookBoard> boardsBySfen;
    readonly Dictionary<PlaybookBoard, List<PlaybookBoardTransition>> outgoing = new();
    readonly Dictionary<PlaybookBoard, List<PlaybookBoardTransition>> incoming = new();

    public PlaybookGraph(Playbook playbook, IPlaybookRepository repository)
    {
        this.playbook = playbook;
        this.repository = repository;
        this.boardsBySfen = playbook.Boards.ToDictionary(b => b.Sfen);
        foreach (var transition in playbook.Transitions.OrderBy(t => t.Id.Value))
        {
            ListOf(this.outgoing, transition.FromBoard).Add(transition);
            ListOf(this.incoming, transition.ToBoard).Add(transition);
        }
    }

    public PlaybookBoard Root => this.boardsBySfen[this.playbook.RootSfen];

    public PlaybookBoard FindForceBoard(string sfen)
    {
        return this.boardsBySfen.GetValueOrDefault(sfen) ?? throw new KeyNotFoundException("この定跡に無い局面です");
    }

    public IReadOnlyList<PlaybookBoardTransition> Outgoing(PlaybookBoard board) =>
        this.outgoing.TryGetValue(board, out var list) ? list : [];

    public IReadOnlyList<PlaybookBoardTransition> Incoming(PlaybookBoard board) =>
        this.incoming.TryGetValue(board, out var list) ? list : [];

    public bool IsJunction(PlaybookBoard board) => this.Incoming(board).Count > 1 || this.Outgoing(board).Count > 1;

    // 開始局面からの手順を足す。指せない手が含まれていれば何も足さない。分岐点・合流点になった局面はノードにする
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

        var touched = new List<PlaybookBoard> { this.Root };
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
            ListOf(this.outgoing, from).Add(transition);
            ListOf(this.incoming, to).Add(transition);
            this.repository.AddTransition(transition);
            touched.Add(from);
            touched.Add(to);
            from = to;
        }

        foreach (var board in touched.Where(this.IsJunction))
        {
            this.EnsureNode(board);
        }
    }

    // ノードから出る遷移を辺の 1 本目として外し、開始局面から辿れなくなった局面・遷移・ノードを消す
    public void RemoveEdge(PlaybookBoardTransition first)
    {
        this.DetachTransition(first);
        this.Prune();
    }

    // ノードに入る辺を、それぞれ出発のノードまで遡って丸ごと外す
    public void RemoveEdgesInto(PlaybookNode node)
    {
        var firsts = node.Boards
            .SelectMany(this.Incoming)
            .Select(this.FirstOfEdge)
            .Distinct()
            .ToList();
        foreach (var first in firsts)
        {
            this.DetachTransition(first);
        }
        this.Prune();
    }

    // 局面が載っている辺の 1 本目。ノードの局面なら、その局面に入る遷移から遡る
    public PlaybookBoardTransition FirstOfEdge(PlaybookBoardTransition transition)
    {
        var current = transition;
        while (current.FromBoard.Node == null)
        {
            current = this.Incoming(current.FromBoard).Single();
        }
        return current;
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

    // 開始局面・分岐点・合流点のノードを解除すると、ノードでない局面の遷移が 1 本以下という前提が崩れる
    public bool CanUnlink(PlaybookNode node)
    {
        return node.Boards.All(b => b.Sfen != this.playbook.RootSfen && !this.IsJunction(b));
    }

    public void Unlink(PlaybookNode node)
    {
        if (!this.CanUnlink(node)) throw new InvalidOperationException("開始局面・分岐点・合流点のノードは解除できません");

        foreach (var board in node.Boards.ToList())
        {
            board.Node = null;
            board.NodeId = null;
            foreach (var transition in this.Outgoing(board)) transition.Condition = "";
        }
        node.Boards.Clear();
        this.playbook.Nodes.Remove(node);
        this.repository.RemoveNode(node);
    }

    // まとめたノードから局面を外し、別のノードにする
    public void Split(PlaybookBoard board)
    {
        var node = board.Node;
        if (node == null || node.Boards.Count <= 1) throw new InvalidOperationException("この局面だけのノードは分けられません");

        node.Boards.Remove(board);
        board.Node = null;
        board.NodeId = null;
        this.EnsureNode(board);
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
        ListOf(this.outgoing, transition.FromBoard).Remove(transition);
        ListOf(this.incoming, transition.ToBoard).Remove(transition);
        this.playbook.Transitions.Remove(transition);
        this.repository.RemoveTransition(transition);
    }

    void Prune()
    {
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
            if (board.Node is not { } node) continue;

            node.Boards.Remove(board);
            board.Node = null;
            board.NodeId = null;
            if (node.Boards.Count > 0) continue;
            this.playbook.Nodes.Remove(node);
            this.repository.RemoveNode(node);
        }
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

    static List<PlaybookBoardTransition> ListOf(Dictionary<PlaybookBoard, List<PlaybookBoardTransition>> map, PlaybookBoard board)
    {
        if (!map.TryGetValue(board, out var list))
        {
            list = new List<PlaybookBoardTransition>();
            map[board] = list;
        }
        return list;
    }
}
