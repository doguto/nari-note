using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly CreateArticleService _createArticleService;
    
    public ArticlesController(CreateArticleService createArticleService)
    {
        _createArticleService = createArticleService;
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
        
        var response = await _createArticleService.ExecuteAsync(request);
        return CreatedAtAction(nameof(GetArticle), new { id = response.Id }, response);
    }
    
    // Placeholder for GetArticle action to satisfy CreatedAtAction
    [HttpGet("{id}")]
    public ActionResult GetArticle(int id)
    {
        return NotFound();
    }
}
