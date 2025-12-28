using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;

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
    
    /// <summary>
    /// ユーザー登録
    /// </summary>
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
        
        var response = await signUpService.ExecuteAsync(request);
        return Ok(response);
    }
    
    /// <summary>
    /// ログイン
    /// </summary>
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
        
        var response = await signInService.ExecuteAsync(request);
        return Ok(response);
    }
}
