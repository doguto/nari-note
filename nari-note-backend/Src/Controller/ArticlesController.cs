using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    readonly CreateArticleService createArticleService;
    readonly GetArticlesByAuthorService getArticlesByAuthorService;
    readonly GetArticlesByTagService getArticlesByTagService;
    readonly GetArticleService getArticleService;
    readonly DeleteArticleService deleteArticleService;
    
    public ArticlesController(
        CreateArticleService createArticleService,
        GetArticlesByAuthorService getArticlesByAuthorService,
        GetArticlesByTagService getArticlesByTagService,
        GetArticleService getArticleService,
        DeleteArticleService deleteArticleService)
    {
        this.createArticleService = createArticleService;
        this.getArticlesByAuthorService = getArticlesByAuthorService;
        this.getArticlesByTagService = getArticlesByTagService;
        this.getArticleService = getArticleService;
        this.deleteArticleService = deleteArticleService;
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
        var article = await getArticleService.ExecuteAsync(id);
        return Ok(article);
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
}
