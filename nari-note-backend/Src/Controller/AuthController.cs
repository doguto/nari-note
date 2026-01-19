using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Security;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ApplicationController
{
    readonly SignUpService signUpService;
    readonly SignInService signInService;
    readonly ICookieOptionsHelper cookieOptionsHelper;
    
    public AuthController(
        SignUpService signUpService,
        SignInService signInService,
        ICookieOptionsHelper cookieOptionsHelper)
    {
        this.signUpService = signUpService;
        this.signInService = signInService;
        this.cookieOptionsHelper = cookieOptionsHelper;
    }
    
    [HttpPost("signup")]
    [AllowAnonymous]
    [ValidateModelState]
    public async Task<ActionResult<AuthResponse>> SignUp([FromBody] SignUpRequest request)
    {
        var response = await signUpService.ExecuteAsync(request, Response);
        return Ok(response);
    }
    
    [HttpPost("signin")]
    [AllowAnonymous]
    [ValidateModelState]
    public async Task<ActionResult<AuthResponse>> SignIn([FromBody] SignInRequest request)
    {
        var response = await signInService.ExecuteAsync(request, Response);
        return Ok(response);
    }
    
    [HttpGet("me")]
    [OptionalAuth]
    public ActionResult<AuthResponse> GetCurrentUser()
    {
        var userId = this.UserId;
        if (userId == null)
        {
            return Ok(new AuthResponse { UserId = null });
        }
        
        return Ok(new AuthResponse { UserId = userId });
    }
    
    [HttpPost("logout")]
    [OptionalAuth]
    public ActionResult Logout()
    {
        // Cookieを削除
        Response.Cookies.Delete("authToken");
        
        return NoContent();
    }
}
