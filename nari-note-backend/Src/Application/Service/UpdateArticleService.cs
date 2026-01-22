using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class UpdateArticleService
{
    readonly IArticleRepository articleRepository;

    public UpdateArticleService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }

    public async Task<UpdateArticleResponse> ExecuteAsync(UserId userId, UpdateArticleRequest request)
    {
        var article = await articleRepository.FindForceByIdAsync(request.Id);
        if (article.AuthorId != userId) throw new UnauthorizedAccessException("この記事を更新する権限がありません");

        // nullでない値のみ更新
        if (!request.Title.IsNullOrEmpty()) article.Title = request.Title!;
        if (!request.Body.IsNullOrEmpty()) article.Body = request.Body!;

        var wasPublished = article.IsPublished;

        // PublishedAtが明示的に指定されている場合はそれを使用
        // 下書きを公開する際に予約投稿日時が未設定の場合は現在時刻を自動設定
        // （既に公開済みの記事を更新する場合は自動設定しない）
        if (request.PublishedAt.HasValue)
        {
            article.PublishedAt = request.PublishedAt.Value;
        }
        else if (!wasPublished && !article.IsPublished)
        {
            article.PublishedAt = DateTime.UtcNow;
        }

        article.UpdatedAt = DateTime.UtcNow;

        // タグと一緒に記事を更新（1回のDB操作に統合）
        await articleRepository.UpdateWithTagAsync(article, request.Tags);

        return new UpdateArticleResponse
        {
            Id = article.Id,
            UpdatedAt = article.UpdatedAt
        };
    }
}
