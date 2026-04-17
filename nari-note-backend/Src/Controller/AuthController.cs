using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ApplicationController
{
    readonly GetCurrentUserService getCurrentUserService;
    readonly LogoutService logoutService;
    readonly SignInService signInService;
    readonly SignUpService signUpService;
    readonly VerifyEmailService verifyEmailService;

    public AuthController(
        SignUpService signUpService,
        SignInService signInService,
        GetCurrentUserService getCurrentUserService,
        LogoutService logoutService,
        VerifyEmailService verifyEmailService
    )
    {
        this.signUpService = signUpService;
        this.signInService = signInService;
        this.getCurrentUserService = getCurrentUserService;
        this.logoutService = logoutService;
        this.verifyEmailService = verifyEmailService;
    }

    [HttpPost("signup")]
    [AllowAnonymous]
    [ValidateModelState]
    public async Task<ActionResult<AuthResponse>> SignUp([FromBody] SignUpRequest request)
    {
        var response = await signUpService.ExecuteAsync(request);
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
    public async Task<ActionResult<AuthResponse>> GetCurrentUser()
    {
        var request = new GetCurrentUserRequest();
        var response = await getCurrentUserService.ExecuteAsync(request, UserId, UserName);
        return Ok(response);
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ValidateModelState]
    public async Task<ActionResult<AuthResponse>> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var response = await verifyEmailService.ExecuteAsync(request, Response);
        return Ok(response);
    }

    [HttpPost("logout")]
    [OptionalAuth]
    public async Task<ActionResult> Logout()
    {
        var request = new LogoutRequest();
        await logoutService.ExecuteAsync(request, Response);
        return NoContent();
    }
}
