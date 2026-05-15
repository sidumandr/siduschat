using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ChatApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [EnableRateLimiting("auth")]    
    [HttpPost("register")]
    public async Task<ActionResult<AuthTokensDto>> Register(
        [FromBody] RegisterRequest req, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(req, ct);
        SetRefreshTokenCookie(result);
        return Ok(new { result.AccessToken, result.ExpiresAt, result.User });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<ActionResult<AuthTokensDto>> Login(
        [FromBody] LoginRequest req, CancellationToken ct)
    {
        var result = await authService.LoginAsync(req, ct);
        SetRefreshTokenCookie(result);
        return Ok(new { result.AccessToken, result.ExpiresAt, result.User });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var token = Request.Cookies["refresh_token"]
            ?? throw new UnauthorizedAccessException("Refresh token bulunamadı.");

        var result = await authService.RefreshTokenAsync(token, ct);
        SetRefreshTokenCookie(result);
        return Ok(new { result.AccessToken, result.ExpiresAt, result.User });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var token = Request.Cookies["refresh_token"];
        if (token is not null)
            await authService.RevokeTokenAsync(token, ct);

        Response.Cookies.Delete("refresh_token");
        return NoContent();
    }

    private void SetRefreshTokenCookie(AuthTokensDto result)
    {
        Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure   = false, // false in development, true in production
            SameSite = SameSiteMode.Strict,
            Expires  = DateTimeOffset.UtcNow.AddDays(30),
        });
    }
}