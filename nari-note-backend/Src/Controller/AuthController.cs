using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    readonly SignUpService signUpService;
    readonly SignInService signInService;
    
    public AuthController(
        SignUpService signUpService,
        SignInService signInService)
    {
        this.signUpService = signUpService;
        this.signInService = signInService;
    }
    
    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponse>> SignUp([FromBody] SignUpRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
            );
            return BadRequest(new { errors });
        }
        
        // 例外はグローバルミドルウェアがキャッチするので、try-catchは不要
        var response = await signUpService.ExecuteAsync(request);
        return Ok(response);
    }
    
    [HttpPost("signin")]
    public async Task<ActionResult<AuthResponse>> SignIn([FromBody] SignInRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
            );
            return BadRequest(new { errors });
        }
        
        // 例外はグローバルミドルウェアがキャッチするので、try-catchは不要
        var response = await signInService.ExecuteAsync(request);
        return Ok(response);
    }
}
