using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Application.Service;

public class CreateArticleService
{
    readonly IArticleRepository articleRepository;
    readonly ICourseRepository courseRepository;
    readonly IKifuRepository kifuRepository;
    readonly TimeProvider timeProvider;

    public CreateArticleService(
        IArticleRepository articleRepository,
        ICourseRepository courseRepository,
        IKifuRepository kifuRepository,
        TimeProvider timeProvider
    )
    {
        this.articleRepository = articleRepository;
        this.courseRepository = courseRepository;
        this.kifuRepository = kifuRepository;
        this.timeProvider = timeProvider;
    }


    public async Task<CreateArticleResponse> ExecuteAsync(CreateArticleRequest request)
    {
        // 講座に記事を追加する場合、講座の所有者であることを検証
        if (request.CourseId.HasValue)
        {
            var course = await courseRepository.FindForceByIdAsync(request.CourseId.Value);

            if (course.UserId != request.AuthorId)
            {
                throw new ForbiddenException("この講座に記事を追加する権限がありません");
            }
        }

        DateTime? publishedAt = null;
        if (request.PublishedAt.HasValue)
        {
            publishedAt = request.PublishedAt.Value;
        }
        else if (request.IsPublished)
        {
            publishedAt = timeProvider.UtcNow();
        }

        var article = new Article
        {
            Title = request.Title,
            Body = request.Body,
            AuthorId = request.AuthorId,
            CourseId = request.CourseId,
            ArticleOrder = request.ArticleOrder,
            PublishedAt = publishedAt
        };

        var created = await articleRepository.CreateAsync(article);

        if (request.Tags.Count > 0)
        {
            await articleRepository.UpdateWithTagAsync(created, request.Tags);
        }

        if (request.Kifus.Count > 0)
        {
            var kifus = request.Kifus.Select(k => new Kifu
            {
                ArticleId = created.Id,
                Name = k.Name,
                KifuText = k.KifuText,
                SortOrder = k.SortOrder
            }).ToList();
            await kifuRepository.ReplaceAllByArticleIdAsync(created.Id, kifus);
        }

        return new CreateArticleResponse
        {
            Id = created.Id,
            CreatedAt = created.CreatedAt
        };
    }
}
