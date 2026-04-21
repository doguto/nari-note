using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ApplicationController
{
    readonly ForgotPasswordService forgotPasswordService;
    readonly GetCurrentUserService getCurrentUserService;
    readonly LogoutService logoutService;
    readonly ResetPasswordService resetPasswordService;
    readonly SignInService signInService;
    readonly SignUpService signUpService;
    readonly UpdatePasswordService updatePasswordService;
    readonly VerifyEmailService verifyEmailService;

    public AuthController(
        SignUpService signUpService,
        SignInService signInService,
        GetCurrentUserService getCurrentUserService,
        LogoutService logoutService,
        VerifyEmailService verifyEmailService,
        UpdatePasswordService updatePasswordService,
        ForgotPasswordService forgotPasswordService,
        ResetPasswordService resetPasswordService
    )
    {
        this.signUpService = signUpService;
        this.signInService = signInService;
        this.getCurrentUserService = getCurrentUserService;
        this.logoutService = logoutService;
        this.verifyEmailService = verifyEmailService;
        this.updatePasswordService = updatePasswordService;
        this.forgotPasswordService = forgotPasswordService;
        this.resetPasswordService = resetPasswordService;
    }

    [HttpPost("signup")]
    [AllowAnonymous]
    [ValidateModelState]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> SignUp([FromBody] SignUpRequest request)
    {
        var response = await signUpService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpPost("signin")]
    [AllowAnonymous]
    [ValidateModelState]
    [EnableRateLimiting("auth")]
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

    [HttpPut("password")]
    [RequireAuth]
    [ValidateModelState]
    public async Task<ActionResult<UpdatePasswordResponse>> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        var response = await updatePasswordService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ValidateModelState]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var response = await forgotPasswordService.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ValidateModelState]
    public async Task<ActionResult<ResetPasswordResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var response = await resetPasswordService.ExecuteAsync(request);
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
