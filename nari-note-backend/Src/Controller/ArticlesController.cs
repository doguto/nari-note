using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ApplicationController
{
    readonly CreateArticleService createArticleService;
    readonly UpdateArticleService updateArticleService;
    readonly GetArticlesByAuthorService getArticlesByAuthorService;
    readonly GetArticlesByTagService getArticlesByTagService;
    readonly GetArticleService getArticleService;
    readonly DeleteArticleService deleteArticleService;
    readonly ToggleLikeService toggleLikeService;
    
    public ArticlesController(
        CreateArticleService createArticleService,
        UpdateArticleService updateArticleService,
        GetArticlesByAuthorService getArticlesByAuthorService,
        GetArticlesByTagService getArticlesByTagService,
        GetArticleService getArticleService,
        DeleteArticleService deleteArticleService,
        ToggleLikeService toggleLikeService)
    {
        this.createArticleService = createArticleService;
        this.updateArticleService = updateArticleService;
        this.getArticlesByAuthorService = getArticlesByAuthorService;
        this.getArticlesByTagService = getArticlesByTagService;
        this.getArticleService = getArticleService;
        this.deleteArticleService = deleteArticleService;
        this.toggleLikeService = toggleLikeService;
    }
    
    [HttpPost]
    [ValidateModelState]
    public async Task<ActionResult> CreateArticle([FromBody] CreateArticleRequest request)
    {
        var response = await createArticleService.ExecuteAsync(request);
        return CreatedAtAction(nameof(GetArticle), new { id = response.Id }, response);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult> GetArticle(int id)
    {
        var request = new GetArticleRequest { Id = id };
        var response = await getArticleService.ExecuteAsync(request);
        return Ok(response);
    }
    
    [HttpPut("{id}")]
    [ValidateModelState]
    public async Task<ActionResult> UpdateArticle(int id, [FromBody] UpdateArticleRequest request)
    {
        request.Id = id;
        var response = await updateArticleService.ExecuteAsync(UserId, request);
        return Ok(response);
    }
    
    [HttpGet("author/{authorId}")]
    public async Task<ActionResult<GetArticlesByAuthorResponse>> GetArticlesByAuthor(int authorId)
    {
        var request = new GetArticlesByAuthorRequest { AuthorId = authorId };
        var response = await getArticlesByAuthorService.ExecuteAsync(request);
        return Ok(response);
    }
    
    [HttpGet("tag/{tagName}")]
    public async Task<ActionResult<GetArticlesByTagResponse>> GetArticlesByTag(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            return BadRequest(new { message = "Tag name cannot be empty" });
        }
        
        var request = new GetArticlesByTagRequest { TagName = tagName };
        var response = await getArticlesByTagService.ExecuteAsync(request);
        return Ok(response);
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteArticle(int id, [FromQuery] int userId)
    {
        var request = new DeleteArticleRequest { Id = id, UserId = userId };
        await deleteArticleService.ExecuteAsync(request);
        return NoContent();
    }
    
    [HttpPost("{id}/like")]
    public async Task<ActionResult> ToggleLike(int id)
    {
        var request = new ToggleLikeRequest 
        { 
            ArticleId = id
        };
        var response = await this.toggleLikeService.ExecuteAsync(UserId, request);
        return Ok(response);
    }
}
