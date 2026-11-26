using NariNoteBackend.Domain.Shogi;

namespace NariNoteBackend.Tests.Domain.Shogi;

public class ShogiBoardTest
{
    const string KakugawariSample = "ln5nl/1r2gkg2/p1psp1spp/3p1pp2/1p5P1/2P1SPP2/PPSPP1N1P/2G4R1/LN2KG2L b Bb 27";

    [Fact]
    public void 平手の初期局面の合法手は30通り()
    {
        var position = ShogiBoard.FromSfen(ShogiBoard.Hirate);

        Assert.Equal(30, position.LegalMoves().Count);
    }

    [Fact]
    public void SFENは手数を1にそろえて書き出す()
    {
        Assert.Equal(
            "ln5nl/1r2gkg2/p1psp1spp/3p1pp2/1p5P1/2P1SPP2/PPSPP1N1P/2G4R1/LN2KG2L b Bb 1",
            ShogiBoard.Normalize(KakugawariSample)
        );
    }

    [Fact]
    public void 駒を取ると成りを戻して持駒に加わる()
    {
        var position = ShogiBoard.FromSfen(ShogiBoard.Hirate);
        foreach (var usi in new[] { "7g7f", "3c3d", "8h2b+" })
        {
            position = position.Apply(ShogiMove.Parse(usi));
        }

        Assert.Equal("lnsgkgsnl/1r5+B1/pppppp1pp/6p2/9/2P6/PP1PPPPPP/7R1/LNSGKGSNL w B 1", position.ToSfen());
    }

    [Fact]
    public void 二歩になる歩は打てない()
    {
        var position = ShogiBoard.FromSfen("lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b P 1");

        Assert.DoesNotContain(position.LegalMoves(), m => m.Drop == PieceKind.Pawn);
    }

    [Fact]
    public void 打ち歩詰めは指せないが詰まない歩打ちは指せる()
    {
        var position = ShogiBoard.FromSfen("8k/9/6NG1/9/9/9/9/9/K8 b P 1");
        var moves = position.LegalMoves().Select(m => m.ToUsi()).ToList();

        Assert.DoesNotContain("P*1b", moves);
        Assert.Contains("P*1d", moves);
        Assert.Equal(78, moves.Count);
    }

    [Fact]
    public void 行き所のない駒は成りしか生成しない()
    {
        var sente = ShogiBoard.FromSfen("k8/4P4/9/9/9/9/9/9/K8 b - 1");
        var gote = ShogiBoard.FromSfen("8k/9/9/9/9/9/9/4p4/K8 w - 1");

        Assert.Equal(["5b5a+"], sente.LegalMoves().Where(m => m.From == ShogiBoard.ParseSquare("5b")).Select(m => m.ToUsi()));
        Assert.Equal(["5h5i+"], gote.LegalMoves().Where(m => m.From == ShogiBoard.ParseSquare("5h")).Select(m => m.ToUsi()));
    }

    [Fact]
    public void 自玉に王手がかかる手は指せない()
    {
        // 5 筋を離れると飛車に玉が素通しになるので、5七の銀はまっすぐ上がる手しか指せない
        var position = ShogiBoard.FromSfen("k3r4/9/9/9/9/9/4S4/9/4K4 b - 1");

        var moves = position.LegalMoves().Where(m => m.From == ShogiBoard.ParseSquare("5g")).Select(m => m.ToUsi());

        Assert.Equal(["5g5f"], moves);
    }

    [Fact]
    public void 手番でない側に王手がかかっている局面は手番として正しくない()
    {
        Assert.False(ShogiBoard.FromSfen("4k4/4P4/9/9/9/9/9/9/K8 b - 1").IsValidTurn());
        Assert.True(ShogiBoard.FromSfen(ShogiBoard.Hirate).IsValidTurn());
    }

    [Theory]
    [InlineData("lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL")]
    [InlineData("lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL x - 1")]
    [InlineData("lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPPP/1B5R1/LNSGKGSNL b - 1")]
    public void 形式の正しくないSFENはArgumentExceptionになる(string sfen)
    {
        Assert.Throws<ArgumentException>(() => ShogiBoard.FromSfen(sfen));
    }
}
