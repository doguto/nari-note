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

    public static PlaybookNodeBoard FindBoard(Playbook playbook, PlaybookNodeBoardId id)
    {
        return playbook.Boards.FirstOrDefault(b => b.Id == id)
            ?? throw new KeyNotFoundException($"盤面{id}が存在しません");
    }

    // ノードの代表は、ノード内で最初に登録された盤面とする
    public static PlaybookNodeBoard Representative(PlaybookNode node)
    {
        return node.Boards.MinBy(b => b.Id.Value)!;
    }

    // 1 手ごとの木を、盤面と手順に畳み直して反映する。手順は出発の盤面と 1 手目で一意なので、その組で既存の行を引き継ぐ。
    // EF Core は削除した盤面を参照する手順も即座に削除扱いにするため、手順の付け替えを済ませてから盤面を消す
    public static void Rebuild(Playbook playbook, PlaybookTree tree, IPlaybookRepository repository)
    {
        var boardsBySfen = playbook.Boards.ToDictionary(b => b.Sfen);
        var layout = tree.Compress(sfen => boardsBySfen.TryGetValue(sfen, out var b) && b.Node.HasContent);

        foreach (var sfen in layout.Boards.Where(s => !boardsBySfen.ContainsKey(s)))
        {
            var node = new PlaybookNode { Playbook = playbook };
            var board = new PlaybookNodeBoard { Playbook = playbook, Node = node, Sfen = sfen };
            node.Boards.Add(board);
            playbook.Nodes.Add(node);
            playbook.Boards.Add(board);
            boardsBySfen[sfen] = board;
            repository.AddNode(node);
            repository.AddBoard(board);
        }

        var existing = playbook.Steps.ToDictionary(s => (s.FromBoard.Sfen, s.Usis[0]));
        var kept = new HashSet<PlaybookNodeStep>();
        foreach (var planned in layout.Steps)
        {
            var to = planned.ToSfen is { } toSfen ? boardsBySfen[toSfen] : null;
            if (existing.TryGetValue((planned.FromSfen, planned.Usis[0]), out var step))
            {
                if (!step.Usis.SequenceEqual(planned.Usis)) step.Usis = planned.Usis;
                step.ToBoard = to;
                step.ToBoardId = to?.Id;
                kept.Add(step);
                continue;
            }

            var created = new PlaybookNodeStep { Playbook = playbook, FromBoard = boardsBySfen[planned.FromSfen], ToBoard = to, Usis = planned.Usis };
            playbook.Steps.Add(created);
            repository.AddStep(created);
            kept.Add(created);
        }

        foreach (var step in playbook.Steps.Where(s => !kept.Contains(s)).ToList())
        {
            playbook.Steps.Remove(step);
            repository.RemoveStep(step);
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
}
