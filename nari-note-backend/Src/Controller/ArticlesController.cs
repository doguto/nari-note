using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ApplicationController
{
    readonly CreateArticleService createArticleService;
    readonly CreateCommentService createCommentService;
    readonly DeleteArticleService deleteArticleService;
    readonly GetArticleContentService getArticleContentService;
    readonly GetArticlesByAuthorService getArticlesByAuthorService;
    readonly GetArticlesByTagService getArticlesByTagService;
    readonly GetArticlesService getArticlesService;
    readonly GetDraftArticlesService getDraftArticlesService;
    readonly GetMyArticlesService getMyArticlesService;
    readonly SearchArticlesService searchArticlesService;
    readonly ToggleLikeService toggleLikeService;
    readonly UpdateArticleService updateArticleService;

    public ArticlesController(
        CreateArticleService createArticleService,
        UpdateArticleService updateArticleService,
        GetArticlesService getArticlesService,
        GetArticlesByAuthorService getArticlesByAuthorService,
        GetArticlesByTagService getArticlesByTagService,
        GetArticleContentService getArticleContentService,
        DeleteArticleService deleteArticleService,
        ToggleLikeService toggleLikeService,
        GetDraftArticlesService getDraftArticlesService,
        SearchArticlesService searchArticlesService,
        CreateCommentService createCommentService,
        GetMyArticlesService getMyArticlesService
    )
    {
        this.createArticleService = createArticleService;
        this.updateArticleService = updateArticleService;
        this.getArticlesService = getArticlesService;
        this.getArticlesByAuthorService = getArticlesByAuthorService;
        this.getArticlesByTagService = getArticlesByTagService;
        this.getArticleContentService = getArticleContentService;
        this.deleteArticleService = deleteArticleService;
        this.toggleLikeService = toggleLikeService;
        this.getDraftArticlesService = getDraftArticlesService;
        this.searchArticlesService = searchArticlesService;
        this.createCommentService = createCommentService;
        this.getMyArticlesService = getMyArticlesService;
    }

    [HttpGet]
    [Auth(AuthLevel.Anonymous)]
    public async Task<ActionResult<GetArticlesResponse>> GetArticles(
        [FromQuery] int limit = 20, [FromQuery] int offset = 0
    )
    {
        var request = new GetArticlesRequest { Limit = limit, Offset = offset };
        var response = await getArticlesService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpPost]
    [Auth(AuthLevel.VerifiedEmail)]
    [ValidateModelState]
    public async Task<ActionResult> CreateArticle([FromBody] CreateArticleRequest request)
    {
        request.AuthorId = UserId!.Value;
        var response = await createArticleService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Auth(AuthLevel.Optional)]
    public async Task<ActionResult<GetArticleContentResponse>> GetArticleContent(ArticleId id)
    {
        var request = new GetArticleContentRequest { Id = id };
        var response = await getArticleContentService.ExecuteAsync(request, UserId);
        return Ok(response);
    }

    [HttpPut("{id}")]
    [Auth(AuthLevel.VerifiedEmail)]
    [ValidateModelState]
    public async Task<ActionResult<UpdateArticleResponse>> UpdateArticle(ArticleId id, [FromBody] UpdateArticleRequest request)
    {
        request.Id = id;
        var response = await updateArticleService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpGet("author/{authorId}")]
    [Auth(AuthLevel.Anonymous)]
    public async Task<ActionResult<GetArticlesByAuthorResponse>> GetArticlesByAuthor(UserId authorId)
    {
        var request = new GetArticlesByAuthorRequest { AuthorId = authorId };
        var response = await getArticlesByAuthorService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpGet("tag/{tagName}")]
    [Auth(AuthLevel.Anonymous)]
    public async Task<ActionResult<GetArticlesByTagResponse>> GetArticlesByTag(string tagName)
    {
        var request = new GetArticlesByTagRequest { TagName = tagName };
        var response = await getArticlesByTagService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult> DeleteArticle(ArticleId id)
    {
        var request = new DeleteArticleRequest { Id = id };
        await deleteArticleService.ExecuteAsync(UserId!.Value, request);
        return NoContent();
    }

    [HttpPost("{id}/like")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<ToggleLikeResponse>> ToggleLike(ArticleId id)
    {
        var request = new ToggleLikeRequest
        {
            ArticleId = id
        };
        var response = await toggleLikeService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpGet("my")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<GetMyArticlesResponse>> GetMyArticles()
    {
        var response = await getMyArticlesService.ExecuteAsync(UserId!.Value);
        return Ok(response);
    }

    [HttpGet("drafts")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<GetDraftArticlesResponse>> GetDraftArticles()
    {
        var response = await getDraftArticlesService.ExecuteAsync(UserId!.Value);
        return Ok(response);
    }

    [HttpGet("search")]
    [Auth(AuthLevel.Anonymous)]
    [ValidateModelState]
    public async Task<ActionResult<SearchArticlesResponse>> SearchArticles([FromQuery] SearchArticlesRequest request)
    {
        var response = await searchArticlesService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpPost("{id}/comments")]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<CreateCommentResponse>> CreateComment(ArticleId id, [FromBody] CreateCommentRequest request)
    {
        request.ArticleId = id;
        var response = await createCommentService.ExecuteAsync(UserId!.Value, request);
        return Created($"/api/articles/{id}/comments/{response.Id}", response);
    }
}
