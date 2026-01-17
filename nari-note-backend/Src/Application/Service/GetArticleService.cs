using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class GetArticleService
{
    readonly IArticleRepository articleRepository;
    readonly ICommentRepository commentRepository;

    public GetArticleService(IArticleRepository articleRepository, ICommentRepository commentRepository)
    {
        this.articleRepository = articleRepository;
        this.commentRepository = commentRepository;
    }

    public async Task<GetArticleResponse> ExecuteAsync(GetArticleRequest request)
    {
        var article = await articleRepository.FindForceByIdAsync(request.Id);
        var comments = await commentRepository.FindByArticleAsync(request.Id);

        return new GetArticleResponse
        {
            Id = article.Id,
            Title = article.Title,
            Body = article.Body,
            AuthorId = article.AuthorId,
            AuthorName = article.Author?.Name ?? "",
            Tags = article.ArticleTags.Select(at => at.Tag?.Name ?? string.Empty).Where(name => !string.IsNullOrEmpty(name)).ToList(),
            LikeCount = article.LikeCount,
            IsPublished = article.IsPublished,
            PublishedAt = article.PublishedAt,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt,
            Comments = comments
                .Where(c => !string.IsNullOrEmpty(c.User?.Name))
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserName = c.User!.Name,
                    Message = c.Message,
                    CreatedAt = c.CreatedAt
                }).OrderBy(c => c.CreatedAt).ToList()
        };
    }
}
