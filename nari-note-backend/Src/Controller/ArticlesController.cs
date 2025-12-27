using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    readonly CreateArticleService createArticleService;
    readonly GetArticlesByAuthorService getArticlesByAuthorService;
    readonly GetArticleService getArticleService;
    readonly DeleteArticleService deleteArticleService;
    
    public ArticlesController(
        CreateArticleService createArticleService,
        GetArticlesByAuthorService getArticlesByAuthorService,
        GetArticleService getArticleService,
        DeleteArticleService deleteArticleService)
    {
        this.createArticleService = createArticleService;
        this.getArticlesByAuthorService = getArticlesByAuthorService;
        this.getArticleService = getArticleService;
        this.deleteArticleService = deleteArticleService;
    }
    
    [HttpPost]
    public async Task<ActionResult> CreateArticle([FromBody] CreateArticleRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
            );
            return BadRequest(new CreateArticleBadRequestResponse { Errors = errors });
        }
        
        // 例外はグローバルミドルウェアがキャッチするので、try-catchは不要
        var response = await this.createArticleService.ExecuteAsync(request);
        return CreatedAtAction(nameof(GetArticle), new { id = response.Id }, response);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult> GetArticle(int id)
    {
        // 例外はグローバルミドルウェアがキャッチするので、try-catchは不要
        var article = await this.getArticleService.ExecuteAsync(id);
        return Ok(article);
    }
    
    [HttpGet("author/{authorId}")]
    public async Task<ActionResult<GetArticlesByAuthorResponse>> GetArticlesByAuthor(int authorId)
    {
        // 例外はグローバルミドルウェアがキャッチするので、try-catchは不要
        var request = new GetArticlesByAuthorRequest { AuthorId = authorId };
        var response = await this.getArticlesByAuthorService.ExecuteAsync(request);
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
