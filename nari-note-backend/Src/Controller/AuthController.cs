using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Filter;

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
    [ValidateModelState]
    public async Task<ActionResult<AuthResponse>> SignUp([FromBody] SignUpRequest request)
    {
        var response = await signUpService.ExecuteAsync(request);
        
        // HttpOnly CookieにトークンをセットするロジックはSignUpServiceに委譲
        // 将来的にCookie対応を追加する際はここで設定
        
        return Ok(response);
    }
    
    [HttpPost("signin")]
    [ValidateModelState]
    public async Task<ActionResult<AuthResponse>> SignIn([FromBody] SignInRequest request)
    {
        var response = await signInService.ExecuteAsync(request);
        
        // HttpOnly CookieにトークンをセットするロジックはSignInServiceに委譲
        // 将来的にCookie対応を追加する際はここで設定
        
        return Ok(response);
    }
}
