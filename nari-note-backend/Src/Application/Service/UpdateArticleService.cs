using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class UpdateArticleService
{
    readonly IArticleRepository articleRepository;
    readonly ICourseRepository courseRepository;
    readonly IKifuRepository kifuRepository;

    public UpdateArticleService(
        IArticleRepository articleRepository,
        ICourseRepository courseRepository,
        IKifuRepository kifuRepository)
    {
        this.articleRepository = articleRepository;
        this.courseRepository = courseRepository;
        this.kifuRepository = kifuRepository;
    }

    public async Task<UpdateArticleResponse> ExecuteAsync(UserId userId, UpdateArticleRequest request)
    {
        var article = await articleRepository.FindForceByIdAsync(request.Id);
        if (article.AuthorId != userId) throw new UnauthorizedAccessException("この記事を更新する権限がありません");

        // 講座記事の場合、講座の所有権を検証
        if (article.CourseId.HasValue)
        {
            var course = await courseRepository.FindForceByIdAsync(article.CourseId.Value);
            if (course.UserId != userId) throw new UnauthorizedAccessException("この講座の記事を更新する権限がありません");
        }

        // nullでない値のみ更新
        if (!request.Title.IsNullOrEmpty()) article.Title = request.Title!;
        if (!request.Body.IsNullOrEmpty()) article.Body = request.Body!;

        // ArticleOrderの更新（講座記事の順序変更）
        if (request.ArticleOrder.HasValue)
        {
            if (!article.CourseId.HasValue)
            {
                throw new InvalidOperationException("講座に所属していない記事の順序は変更できません");
            }
            article.ArticleOrder = request.ArticleOrder.Value;
        }

        var wasPublished = article.IsPublished;

        // PublishedAtが明示的に指定されている場合はそれを使用
        // 下書きを公開する際に予約投稿日時が未設定の場合は現在時刻を自動設定
        // （既に公開済みの記事を更新する場合は自動設定しない）
        if (request.PublishedAt.HasValue)
        {
            article.PublishedAt = request.PublishedAt!;
        }
        else if (!wasPublished && request.IsPublished.HasValue && request.IsPublished.Value)
        {
            article.PublishedAt = DateTime.UtcNow;
        }

        article.UpdatedAt = DateTime.UtcNow;

        await articleRepository.UpdateWithTagAsync(article, request.Tags);

        if (request.Kifus != null)
        {
            var kifus = request.Kifus.Select(k => new Kifu
            {
                ArticleId = article.Id,
                KifuText = k.KifuText,
                SortOrder = k.SortOrder
            }).ToList();
            await kifuRepository.ReplaceAllByArticleIdAsync(article.Id, kifus);
        }

        return new UpdateArticleResponse
        {
            Id = article.Id,
            UpdatedAt = article.UpdatedAt
        };
    }
}
