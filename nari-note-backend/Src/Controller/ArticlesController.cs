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
    readonly UpdateArticleService updateArticleService;
    readonly GetArticlesService getArticlesService;
    readonly GetArticlesByAuthorService getArticlesByAuthorService;
    readonly GetArticlesByTagService getArticlesByTagService;
    readonly GetArticleService getArticleService;
    readonly DeleteArticleService deleteArticleService;
    readonly ToggleLikeService toggleLikeService;
    readonly GetDraftArticlesService getDraftArticlesService;
    readonly SearchArticlesService searchArticlesService;
    
    public ArticlesController(
        CreateArticleService createArticleService,
        UpdateArticleService updateArticleService,
        GetArticlesService getArticlesService,
        GetArticlesByAuthorService getArticlesByAuthorService,
        GetArticlesByTagService getArticlesByTagService,
        GetArticleService getArticleService,
        DeleteArticleService deleteArticleService,
        ToggleLikeService toggleLikeService,
        GetDraftArticlesService getDraftArticlesService,
        SearchArticlesService searchArticlesService)
    {
        this.createArticleService = createArticleService;
        this.updateArticleService = updateArticleService;
        this.getArticlesService = getArticlesService;
        this.getArticlesByAuthorService = getArticlesByAuthorService;
        this.getArticlesByTagService = getArticlesByTagService;
        this.getArticleService = getArticleService;
        this.deleteArticleService = deleteArticleService;
        this.toggleLikeService = toggleLikeService;
        this.getDraftArticlesService = getDraftArticlesService;
        this.searchArticlesService = searchArticlesService;
    }
    
    [HttpGet]
    public async Task<ActionResult<GetArticlesResponse>> GetArticles([FromQuery] int limit = 20, [FromQuery] int offset = 0)
    {
        var request = new GetArticlesRequest { Limit = limit, Offset = offset };
        var response = await getArticlesService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpPost]
    [ValidateModelState]
    public async Task<ActionResult> CreateArticle([FromBody] CreateArticleRequest request)
    {
        request.AuthorId = UserId;
        var response = await createArticleService.ExecuteAsync(request);
        return CreatedAtAction(nameof(GetArticle), new { id = response.Id }, response);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult> GetArticle(ArticleId id)
    {
        var request = new GetArticleRequest { Id = id };
        var response = await getArticleService.ExecuteAsync(request);
        return Ok(response);
    }
    
    [HttpPut("{id}")]
    [ValidateModelState]
    public async Task<ActionResult> UpdateArticle(ArticleId id, [FromBody] UpdateArticleRequest request)
    {
        request.Id = id;
        var response = await updateArticleService.ExecuteAsync(UserId, request);
        return Ok(response);
    }

    [HttpGet("author/{authorId}")]
    public async Task<ActionResult<GetArticlesByAuthorResponse>> GetArticlesByAuthor(UserId authorId)
    {
        var request = new GetArticlesByAuthorRequest { AuthorId = authorId };
        var response = await getArticlesByAuthorService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpGet("tag/{tagName}")]
    public async Task<ActionResult<GetArticlesByTagResponse>> GetArticlesByTag(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName)) return BadRequest(new { message = "Tag name cannot be empty" });

        var request = new GetArticlesByTagRequest { TagName = tagName };
        var response = await getArticlesByTagService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteArticle(ArticleId id)
    {
        var request = new DeleteArticleRequest { Id = id };
        await deleteArticleService.ExecuteAsync(UserId, request);
        return NoContent();
    }

    [HttpPost("{id}/like")]
    public async Task<ActionResult> ToggleLike(ArticleId id)
    {
        var request = new ToggleLikeRequest 
        { 
            ArticleId = id
        };
        var response = await toggleLikeService.ExecuteAsync(UserId, request);
        return Ok(response);
    }

    [HttpGet("drafts")]
    public async Task<ActionResult<GetDraftArticlesResponse>> GetDraftArticles()
    {
        var response = await getDraftArticlesService.ExecuteAsync(UserId);
        return Ok(response);
    }

    [HttpGet("search")]
    [ValidateModelState]
    public async Task<ActionResult<SearchArticlesResponse>> SearchArticles([FromQuery] SearchArticlesRequest request)
    {
        var response = await searchArticlesService.ExecuteAsync(request);
        return Ok(response);
    }
}
