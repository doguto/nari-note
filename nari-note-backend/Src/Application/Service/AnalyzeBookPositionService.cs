using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Helper;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class AnalyzeBookPositionService
{
    const int PvLength = 8;

    readonly IBookRepository bookRepository;
    readonly IShogiEngineGateway shogiEngineGateway;

    public AnalyzeBookPositionService(IBookRepository bookRepository, IShogiEngineGateway shogiEngineGateway)
    {
        this.bookRepository = bookRepository;
        this.shogiEngineGateway = shogiEngineGateway;
    }

    public async Task<AnalyzeBookPositionResponse> ExecuteAsync(UserId userId, AnalyzeBookPositionRequest request)
    {
        var book = await this.bookRepository.FindForceByIdAsync(request.Id);
        BookAccess.EnsureOwner(book, userId);
        if (!this.shogiEngineGateway.IsEnabled) return new AnalyzeBookPositionResponse { IsAvailable = false };

        var shogi = ShogiPosition.FromSfen(request.Sfen);
        if (!shogi.IsValidTurn()) throw new ArgumentException("手番の側が相手の玉を取れる局面は解析できません");
        var sfen = shogi.ToSfen();
        var sente = shogi.Turn == Side.Sente;

        var lines = await this.shogiEngineGateway.AnalyzeAsync(sfen, request.Engine);
        return new AnalyzeBookPositionResponse
        {
            IsAvailable = true,
            Lines = lines.Select(l =>
            {
                var labels = ShogiNotation.SequenceLabels(sfen, l.Pv.Take(PvLength));
                return new EngineLineDto
                {
                    Usi = l.Usi,
                    Label = labels.FirstOrDefault() ?? l.Usi,
                    Score = sente ? l.Score : -l.Score,
                    Pv = labels,
                };
            }).ToList(),
        };
    }
}
