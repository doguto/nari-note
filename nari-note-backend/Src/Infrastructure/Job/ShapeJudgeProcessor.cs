using System.Net;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Infrastructure.Job;

// 定跡に新しく入った局面について、同じ形かもしれない局面を探し、エンジンで判定する。
// まとめるかどうかは本人が決めるので、ここでは判定結果を残すだけにする
public class ShapeJudgeProcessor
{
    const int ScanBatchSize = 10;
    // 1 局面あたりの判定数。判定は 1 件で数十秒かかるので、差分の小さい候補に絞る
    const int CandidatesPerPosition = 3;

    readonly IBookRepository bookRepository;
    readonly IShogiEngineGateway shogiEngineGateway;
    readonly ILogger<ShapeJudgeProcessor> logger;

    public ShapeJudgeProcessor(
        IBookRepository bookRepository,
        IShogiEngineGateway shogiEngineGateway,
        ILogger<ShapeJudgeProcessor> logger
    )
    {
        this.bookRepository = bookRepository;
        this.shogiEngineGateway = shogiEngineGateway;
        this.logger = logger;
    }

    public bool IsEnabled => this.shogiEngineGateway.IsEnabled;

    public async Task ResetAsync()
    {
        await this.bookRepository.ResetRunningCandidatesAsync();
    }

    // 処理する仕事があれば true を返す
    public async Task<bool> ProcessAsync(CancellationToken cancellationToken)
    {
        if (!this.shogiEngineGateway.IsEnabled) return false;
        if (await this.ScanAsync(cancellationToken)) return true;
        return await this.JudgeNextAsync(cancellationToken);
    }

    async Task<bool> ScanAsync(CancellationToken cancellationToken)
    {
        var unscanned = await this.bookRepository.FindUnscannedPositionsAsync(ScanBatchSize);
        if (unscanned.Count == 0) return false;

        foreach (var bookPositions in unscanned.GroupBy(p => p.BookId))
        {
            var book = await this.bookRepository.FindForceGraphByIdAsync(bookPositions.Key);
            var representatives = book.Positions
                                      .GroupBy(p => p.ShapeGroupId)
                                      .ToDictionary(g => g.Key, g => g.MinBy(p => p.Id.Value)!);
            foreach (var target in bookPositions)
            {
                var position = book.Positions.First(p => p.Id == target.Id);
                var others = representatives.Values.Where(r => r.ShapeGroupId != position.ShapeGroupId).ToList();
                var found = await this.shogiEngineGateway.FindCandidatesAsync(
                    position.Sfen,
                    others.Select(o => o.Sfen).ToList(),
                    cancellationToken
                );

                foreach (var c in found.OrderBy(c => c.UnitCount).Take(CandidatesPerPosition))
                {
                    var rep = others.FirstOrDefault(o => o.Sfen == c.Sfen);
                    if (rep == null) continue;
                    if (book.ShapeCandidates.Any(x => x.PositionAId == rep.Id && x.PositionBId == position.Id)) continue;
                    var candidate = new ShapeCandidate { Book = book, PositionA = rep, PositionB = position };
                    book.ShapeCandidates.Add(candidate);
                    this.bookRepository.AddShapeCandidate(candidate);
                }
                position.CandidatesScanned = true;
            }
            await this.bookRepository.SaveAsync();
        }
        return true;
    }

    async Task<bool> JudgeNextAsync(CancellationToken cancellationToken)
    {
        var candidate = await this.bookRepository.FindNextQueuedCandidateAsync();
        if (candidate == null) return false;

        candidate.Status = ShapeCandidateStatus.Running;
        await this.bookRepository.SaveAsync();

        try
        {
            var verdict = await this.shogiEngineGateway.JudgeAsync(candidate.PositionA.Sfen, candidate.PositionB.Sfen, cancellationToken);
            candidate.Status = ShapeCandidateStatus.Done;
            candidate.Verdict = verdict.Kind;
            candidate.Delta = verdict.Delta;
            candidate.ConfirmedDepth = verdict.ConfirmedDepth;
            candidate.Reason = Truncate(verdict.Reason);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            // 同時実行数が一時的に埋まっているだけなので、失敗にせず待ち行列に戻して後で判定し直す
            this.logger.LogWarning(ex, "Shape judge throttled. CandidateId={CandidateId}", candidate.Id);
            candidate.Status = ShapeCandidateStatus.Queued;
            await this.bookRepository.SaveAsync();
            return false;
        }
        catch (System.Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            this.logger.LogWarning(ex, "Shape judge failed. CandidateId={CandidateId}", candidate.Id);
            candidate.Status = ShapeCandidateStatus.Failed;
            candidate.Reason = Truncate(ex.Message);
        }
        await this.bookRepository.SaveAsync();
        return true;
    }

    static string Truncate(string text) => text.Length <= 500 ? text : text[..500];
}
