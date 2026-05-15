using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatApp.Application.Features.Auth;

public class AuthService(
    IUnitOfWork            uow,
    IJwtService            jwt,
    IPasswordHasher        hasher,
    ILogger<AuthService>   logger) : IAuthService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    public async Task<AuthTokensDto> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        if (await uow.Users.ExistsAsync(req.Email, req.Username, ct))
            throw new InvalidOperationException("Bu email veya kullanıcı adı zaten alınmış.");

        var user = User.Create(req.Username, req.Email, hasher.Hash(req.Password), req.DisplayName);
        await uow.Users.AddAsync(user, ct);

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthTokensDto> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var user = await uow.Users.GetByEmailAsync(req.Email, ct)
            ?? throw new UnauthorizedAccessException("Geçersiz email veya şifre.");

        if (!hasher.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Geçersiz email veya şifre.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Hesap devre dışı bırakılmış.");

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthTokensDto> RefreshTokenAsync(string rawToken, CancellationToken ct = default)
    {
        var stored = await uow.RefreshTokens.GetActiveTokenAsync(rawToken, ct)
            ?? throw new UnauthorizedAccessException("Refresh token geçersiz veya süresi dolmuş.");

        stored.Revoke("rotated");

        var user = await uow.Users.GetByIdAsync(stored.UserId, ct)
            ?? throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

        return await IssueTokensAsync(user, ct);
    }

    public async Task RevokeTokenAsync(string rawToken, CancellationToken ct = default)
    {
        var stored = await uow.RefreshTokens.GetActiveTokenAsync(rawToken, ct);
        if (stored is null) return; 

        stored.Revoke("logout");
        await uow.SaveChangesAsync(ct);
    }

    
    private async Task<AuthTokensDto> IssueTokensAsync(User user, CancellationToken ct)
    {
        var accessToken  = jwt.GenerateAccessToken(user.Id, user.Username, user.Email);
        var rawRefresh   = jwt.GenerateRefreshToken();
        var expiresAt    = DateTime.UtcNow.Add(RefreshTokenLifetime);

        var refreshToken = RefreshToken.Create(user.Id, rawRefresh, expiresAt);
        await uow.RefreshTokens.AddAsync(refreshToken, ct);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Token üretildi: UserId={UserId}", user.Id);

        return new AuthTokensDto(
            accessToken,
            rawRefresh,
            DateTime.UtcNow.AddMinutes(15),
            ToUserDto(user)
        );
    }

    private static UserDto ToUserDto(User u) =>
        new(u.Id, u.Username, u.DisplayName, u.AvatarUrl, false, u.LastSeenAt);
}