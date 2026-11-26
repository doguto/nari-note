using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class AnalyzePlaybookBoardServiceTest
{
    readonly IPlaybookRepository playbookRepository = Substitute.For<IPlaybookRepository>();
    readonly IShogiEngineGateway shogiEngineGateway = Substitute.For<IShogiEngineGateway>();
    readonly User owner = new UserBuilder().Build();
    readonly PlaybookBuilder builder;
    readonly Playbook playbook;
    readonly AnalyzePlaybookBoardService service;

    public AnalyzePlaybookBoardServiceTest()
    {
        this.builder = new PlaybookBuilder(this.owner);
        this.playbook = this.builder.Build();
        this.playbookRepository.FindForceByIdAsync(this.playbook.Id).Returns(this.playbook);
        this.shogiEngineGateway.IsEnabled.Returns(true);
        this.service = new AnalyzePlaybookBoardService(this.playbookRepository, this.shogiEngineGateway);
    }

    [Fact]
    public async Task 選んだエンジンで解析し読み筋を記譜にし評価値を先手から見た値にする()
    {
        var sfen = this.builder.Line("7g7f");
        this.shogiEngineGateway.AnalyzeAsync(sfen, AnalysisEngine.TsuyouraouMicro).Returns([
            new EngineLine { Usi = "3c3d", Score = 50, Pv = ["3c3d", "8h2b+", "3a2b"] },
        ]);

        var response = await this.service.ExecuteAsync(this.owner.Id, new AnalyzePlaybookBoardRequest { Id = this.playbook.Id, Sfen = sfen, Engine = AnalysisEngine.TsuyouraouMicro });

        Assert.True(response.IsAvailable);
        var line = Assert.Single(response.Lines);
        Assert.Equal("△３四歩", line.Label);
        Assert.Equal(-50, line.Score);
        Assert.Equal(["△３四歩", "▲２二角成", "△同　銀"], line.Pv);
    }

    [Fact]
    public async Task エンジンが無い環境では使えないことを返す()
    {
        this.shogiEngineGateway.IsEnabled.Returns(false);

        var response = await this.service.ExecuteAsync(this.owner.Id, new AnalyzePlaybookBoardRequest { Id = this.playbook.Id, Sfen = this.builder.Root.Sfen, Engine = AnalysisEngine.TsuyouraouMini });

        Assert.False(response.IsAvailable);
        await this.shogiEngineGateway.DidNotReceiveWithAnyArgs().AnalyzeAsync(null!, null!);
    }

    [Fact]
    public async Task 持ち主以外は解析できない()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            this.service.ExecuteAsync(new UserBuilder().Build().Id, new AnalyzePlaybookBoardRequest { Id = this.playbook.Id, Sfen = this.builder.Root.Sfen, Engine = AnalysisEngine.TsuyouraouMini })
        );
    }

    [Fact]
    public async Task 保存していない局面も解析できる()
    {
        const string sfen = "lnsgkgsnl/1r5b1/ppppppppp/9/9/2P6/PP1PPPPPP/1B5R1/LNSGKGSNL w - 1";
        this.shogiEngineGateway.AnalyzeAsync(sfen, AnalysisEngine.TsuyouraouMini).Returns([new EngineLine { Usi = "3c3d", Score = 10, Pv = ["3c3d"] }]);

        var response = await this.service.ExecuteAsync(this.owner.Id, new AnalyzePlaybookBoardRequest { Id = this.playbook.Id, Sfen = sfen, Engine = AnalysisEngine.TsuyouraouMini });

        Assert.Equal("△３四歩", Assert.Single(response.Lines).Label);
    }

    [Fact]
    public async Task 形式の正しくない局面はArgumentExceptionになる()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            this.service.ExecuteAsync(this.owner.Id, new AnalyzePlaybookBoardRequest { Id = this.playbook.Id, Sfen = "xx", Engine = AnalysisEngine.TsuyouraouMini })
        );
    }
}
