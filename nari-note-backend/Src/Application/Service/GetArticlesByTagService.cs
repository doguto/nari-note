using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using NariNoteBackend.Application.Dto;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Repository;

namespace NariNoteBackend.Application.Service;

public class GetArticlesByTagService
{
    static readonly Regex ValidTagPattern = new(@"^[a-zA-Z0-9\u3040-\u30FF\u4E00-\u9FFF_\-\.]+$", RegexOptions.Compiled);

    readonly IArticleRepository articleRepository;

    public GetArticlesByTagService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }

    public async Task<GetArticlesByTagResponse> ExecuteAsync(GetArticlesByTagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TagName) || !ValidTagPattern.IsMatch(request.TagName))
            throw new ValidationException("タグ名に使用できない文字が含まれています");

        var articles = await articleRepository.FindByTagAsync(request.TagName);

        return new GetArticlesByTagResponse
        {
            Articles = articles.Select(a => new ArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                Body = a.Body,
                AuthorId = a.AuthorId,
                AuthorName = a.Author?.Name ?? "",
                Tags = a.ArticleTags.Select(at => at.Tag.Name).ToList(),
                LikeCount = a.Likes.Count,
                IsPublished = a.IsPublished,
                PublishedAt = a.PublishedAt,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList()
        };
    }
}
