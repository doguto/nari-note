using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Helper;

public static class PlaybookAccess
{
    public static void EnsureOwner(Playbook playbook, UserId userId)
    {
        if (!playbook.IsOwnedBy(userId)) throw new ForbiddenException("この定跡を操作する権限がありません");
    }

    public static PlaybookBoard FindBoard(Playbook playbook, PlaybookBoardId id)
    {
        return playbook.Boards.FirstOrDefault(b => b.Id == id)
            ?? throw new KeyNotFoundException($"局面{id}が存在しません");
    }

    public static PlaybookNode FindNode(Playbook playbook, PlaybookNodeId id)
    {
        return playbook.Nodes.FirstOrDefault(n => n.Id == id)
            ?? throw new KeyNotFoundException($"ノード{id}が存在しません");
    }

    public static PlaybookBoardTransition FindTransition(Playbook playbook, PlaybookBoardTransitionId id)
    {
        return playbook.Transitions.FirstOrDefault(t => t.Id == id)
            ?? throw new KeyNotFoundException($"遷移{id}が存在しません");
    }

    // ノードの代表は、ノード内で最初に登録された局面とする
    public static PlaybookBoard Representative(PlaybookNode node)
    {
        return node.Boards.MinBy(b => b.Id.Value)!;
    }

    public static bool ContainsRoot(Playbook playbook, PlaybookNode node)
    {
        return node.Boards.Any(b => b.Sfen == playbook.RootSfen);
    }

    // 局面にノードを作る。辺の途中の局面なら、次の Rebuild でその辺が 2 本に分かれる
    public static PlaybookBoard AddNode(Playbook playbook, string sfen, IPlaybookRepository repository)
    {
        var node = new PlaybookNode { Playbook = playbook };
        var board = new PlaybookBoard { Playbook = playbook, Node = node, Sfen = sfen };
        node.Boards.Add(board);
        playbook.Nodes.Add(node);
        playbook.Boards.Add(board);
        repository.AddNode(node);
        repository.AddBoard(board);
        return board;
    }

    // 1 手ごとのグラフを、ノードの局面と遷移に畳み直して反映する。ノードは解除されるまで残し、unlinked の局面だけを手順の途中に戻す。
    // 遷移は出発の局面と 1 手目で一意なので、その組で既存の行を引き継ぐ。辺が分かれても、つながっても、条件は出発側の辺に残る。
    // EF Core は削除した局面を参照する遷移も即座に削除扱いにするため、遷移の付け替えを済ませてから局面を消す
    public static void Rebuild(Playbook playbook, PlaybookTree tree, IPlaybookRepository repository, IReadOnlySet<string>? unlinked = null)
    {
        var boardsBySfen = playbook.Boards.ToDictionary(b => b.Sfen);
        var layout = tree.Compress(sfen => boardsBySfen.ContainsKey(sfen) && unlinked?.Contains(sfen) != true);

        foreach (var sfen in layout.Boards.Where(s => !boardsBySfen.ContainsKey(s)))
        {
            boardsBySfen[sfen] = AddNode(playbook, sfen, repository);
        }

        var existing = playbook.Transitions.ToDictionary(t => (t.FromBoard.Sfen, t.Usis[0]));
        var kept = new HashSet<PlaybookBoardTransition>();
        foreach (var planned in layout.Transitions)
        {
            var to = planned.ToSfen is { } toSfen ? boardsBySfen[toSfen] : null;
            if (existing.TryGetValue((planned.FromSfen, planned.Usis[0]), out var transition))
            {
                if (!transition.Usis.SequenceEqual(planned.Usis)) transition.Usis = planned.Usis;
                transition.ToBoard = to;
                transition.ToBoardId = to?.Id;
                kept.Add(transition);
                continue;
            }

            var created = new PlaybookBoardTransition { Playbook = playbook, FromBoard = boardsBySfen[planned.FromSfen], ToBoard = to, Usis = planned.Usis };
            playbook.Transitions.Add(created);
            repository.AddTransition(created);
            kept.Add(created);
        }

        foreach (var transition in playbook.Transitions.Where(t => !kept.Contains(t)).ToList())
        {
            playbook.Transitions.Remove(transition);
            repository.RemoveTransition(transition);
        }

        foreach (var board in playbook.Boards.Where(b => !layout.Boards.Contains(b.Sfen)).ToList())
        {
            var node = board.Node;
            node.Boards.Remove(board);
            playbook.Boards.Remove(board);
            repository.RemoveBoard(board);
            if (node.Boards.Count == 0)
            {
                playbook.Nodes.Remove(node);
                repository.RemoveNode(node);
            }
        }
    }

    // ノードを解除して、局面を手順の途中に戻す。前後の辺が 1 本ずつのときだけ、1 本の辺につなげられる
    public static void Unlink(Playbook playbook, PlaybookNode node, PlaybookTree tree, IPlaybookRepository repository)
    {
        if (ContainsRoot(playbook, node)) throw new InvalidOperationException("開始局面のノードは解除できません");
        var sfens = node.Boards.Select(b => b.Sfen).ToHashSet();
        if (sfens.Any(s => tree.InDegreeOf(s) > 1 || tree.ChildrenOf(s).Count > 1))
        {
            throw new InvalidOperationException("分岐点や合流点のノードは解除できません");
        }
        Rebuild(playbook, tree, repository, sfens);
    }
}
