using ChatApp.Application.Features.Auth;
using ChatApp.Application.Features.Chat;
using ChatApp.Application.Features.Rooms;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Interfaces;
using ChatApp.Infrastructure.Persistence;
using ChatApp.Infrastructure.Persistence.Repositories;
using ChatApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using ChatApp.Application.Validators;
using FluentValidation;

namespace ChatApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // PostgreSQL
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(
                config.GetConnectionString("Default"),
                npg => npg.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));

        // Repositories
        services.AddScoped<IUserRepository,         UserRepository>();
        services.AddScoped<IRoomRepository,         RoomRepository>();
        services.AddScoped<IMessageRepository,       MessageRepository>();
        services.AddScoped<IRefreshTokenRepository,  RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork,              UnitOfWork>();

        // Application services
        services.AddScoped<IAuthService,  AuthService>();
        services.AddScoped<IChatService,  ChatService>();
        services.AddScoped<IRoomService,  RoomService>();

        // Infrastructure services
        services.AddSingleton<IPresenceService, RedisPresenceService>();
        services.AddSingleton<IJwtService,      JwtService>();
        services.AddSingleton<IPasswordHasher,  PasswordHasher>();

        // fluent validation
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        return services;
    }
}