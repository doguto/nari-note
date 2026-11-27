using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Shogi;

namespace NariNoteBackend.Application.Helper;

public record PlaybookTreeTransition(string FromSfen, List<string> Usis, string? ToSfen);

public record PlaybookTreeLayout(HashSet<string> Boards, List<PlaybookTreeTransition> Transitions);

// 定跡を 1 手ごとのグラフに展開したもの。手の追加や削除はこの形で行い、Compress でノードの局面と遷移に畳み直す。
// こうしておくと、手順の途中での分岐・合流・延長・削除と、ノードにする・ノードを解除するを同じ規則で扱える
public class PlaybookTree
{
    readonly Dictionary<string, List<(string Usi, string Child)>> children = new();

    public PlaybookTree(string rootSfen)
    {
        this.RootSfen = rootSfen;
    }

    public string RootSfen { get; }

    public static PlaybookTree Load(Playbook playbook)
    {
        var tree = new PlaybookTree(playbook.RootSfen);
        foreach (var transition in playbook.Transitions.OrderBy(s => s.Id.Value))
        {
            var sfen = transition.FromBoard.Sfen;
            foreach (var usi in transition.Usis)
            {
                sfen = tree.Play(sfen, usi);
            }
        }
        return tree;
    }

    public IReadOnlyList<(string Usi, string Child)> ChildrenOf(string sfen) =>
        this.children.TryGetValue(sfen, out var list) ? list : [];

    public bool Contains(string sfen) => this.Reachable().Contains(sfen);

    // 開始局面からの手順を足す。指せない手が含まれていれば何も足さない
    public void AddLine(IReadOnlyList<string> usis)
    {
        var shogi = ShogiBoard.FromSfen(this.RootSfen);
        var played = new List<(string Sfen, string Usi)>();
        foreach (var raw in usis)
        {
            var move = ShogiMove.Parse(raw);
            if (!shogi.IsLegal(move)) throw new ArgumentException($"この局面では指せない手です: {raw}");
            played.Add((shogi.ToSfen(), move.ToUsi()));
            shogi = shogi.Apply(move);
        }
        foreach (var (sfen, usi) in played)
        {
            this.Play(sfen, usi);
        }
    }

    // 手を外し、開始局面から辿れなくなった局面も消す
    public bool RemoveMove(string sfen, string usi)
    {
        if (!this.children.TryGetValue(sfen, out var list)) return false;
        if (list.RemoveAll(c => c.Usi == usi) == 0) return false;

        this.Prune();
        return true;
    }

    void Prune()
    {
        var reachable = this.Reachable();
        foreach (var key in this.children.Keys.Where(k => !reachable.Contains(k)).ToList())
        {
            this.children.Remove(key);
        }
    }

    public int InDegreeOf(string sfen) =>
        this.Reachable().Sum(s => this.ChildrenOf(s).Count(c => c.Child == sfen));

    // 開始局面・分岐点・合流点・keep が true の局面をノードの局面として残し、その間を遷移の手順にまとめる
    public PlaybookTreeLayout Compress(Func<string, bool> keep)
    {
        var order = this.Reachable();
        var inDegree = new Dictionary<string, int>();
        foreach (var sfen in order)
        {
            foreach (var (_, child) in this.ChildrenOf(sfen))
            {
                inDegree[child] = inDegree.GetValueOrDefault(child) + 1;
            }
        }

        var boards = order
            .Where(s => s == this.RootSfen || inDegree.GetValueOrDefault(s) > 1 || this.ChildrenOf(s).Count > 1 || keep(s))
            .ToHashSet();

        var transitions = new List<PlaybookTreeTransition>();
        foreach (var from in order.Where(boards.Contains))
        {
            foreach (var (usi, child) in this.ChildrenOf(from))
            {
                var usis = new List<string> { usi };
                var cursor = child;
                while (!boards.Contains(cursor) && this.ChildrenOf(cursor).Count == 1)
                {
                    var next = this.ChildrenOf(cursor)[0];
                    usis.Add(next.Usi);
                    cursor = next.Child;
                }
                transitions.Add(new PlaybookTreeTransition(from, usis, boards.Contains(cursor) ? cursor : null));
            }
        }
        return new PlaybookTreeLayout(boards, transitions);
    }

    string Play(string sfen, string usi)
    {
        if (!this.children.TryGetValue(sfen, out var list))
        {
            list = new List<(string, string)>();
            this.children[sfen] = list;
        }
        var existing = list.FirstOrDefault(c => c.Usi == usi);
        if (existing.Child != null) return existing.Child;

        var child = ShogiBoard.FromSfen(sfen).Apply(ShogiMove.Parse(usi)).ToSfen();
        list.Add((usi, child));
        return child;
    }

    // 幅優先の順に並べる。手順の並びを保存のたびに変えないため、順序を保つ
    List<string> Reachable()
    {
        var seen = new HashSet<string> { this.RootSfen };
        var order = new List<string> { this.RootSfen };
        for (var i = 0; i < order.Count; i++)
        {
            foreach (var (_, child) in this.ChildrenOf(order[i]))
            {
                if (seen.Add(child)) order.Add(child);
            }
        }
        return order;
    }
}
