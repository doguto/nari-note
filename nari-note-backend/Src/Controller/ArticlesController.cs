using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly CreateArticleService createArticleService;
    private readonly GetArticlesByAuthorService getArticlesByAuthorService;
    
    public ArticlesController(
        CreateArticleService createArticleService,
        GetArticlesByAuthorService getArticlesByAuthorService)
    {
        this.createArticleService = createArticleService;
        this.getArticlesByAuthorService = getArticlesByAuthorService;
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
    
    // TODO: Implement GetArticle method (Issue #XX)
    // Placeholder for GetArticle action to satisfy CreatedAtAction
    [HttpGet("{id}")]
    public ActionResult GetArticle(int id)
    {
        return NotFound();
    }
    
    [HttpGet("author/{authorId}")]
    public async Task<ActionResult<GetArticlesByAuthorResponse>> GetArticlesByAuthor(int authorId)
    {
        var request = new GetArticlesByAuthorRequest { AuthorId = authorId };
        var response = await this.getArticlesByAuthorService.ExecuteAsync(request);
        return Ok(response);
    }
}
