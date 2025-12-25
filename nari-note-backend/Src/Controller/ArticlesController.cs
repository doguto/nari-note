using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly CreateArticleService createArticleService;
    private readonly DeleteArticleService deleteArticleService;
    
    public ArticlesController(
        CreateArticleService createArticleService,
        DeleteArticleService deleteArticleService)
    {
        this.createArticleService = createArticleService;
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
        
        var response = await this.createArticleService.ExecuteAsync(request);
        return CreatedAtAction(nameof(GetArticle), new { id = response.Id }, response);
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteArticle(int id, [FromQuery] int userId)
    {
        try
        {
            var request = new DeleteArticleRequest { Id = id, UserId = userId };
            await this.deleteArticleService.ExecuteAsync(request);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound(new DeleteArticleNotFoundResponse { ArticleId = id });
        }
        catch (ForbiddenException)
        {
            return StatusCode(403, new DeleteArticleForbiddenResponse { ArticleId = id });
        }
    }
    
    // TODO: Implement GetArticle method (Issue #XX)
    // Placeholder for GetArticle action to satisfy CreatedAtAction
    [HttpGet("{id}")]
    public ActionResult GetArticle(int id)
    {
        return NotFound();
    }
}
