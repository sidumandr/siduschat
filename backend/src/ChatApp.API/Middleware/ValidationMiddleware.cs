using FluentValidation;
using System.Text.Json;

namespace ChatApp.API.Middleware;

public static class ValidationExtensions
{
    public static async Task<bool> ValidateAsync<T>(
        this IValidator<T> validator,
        T instance,
        HttpResponse response,
        CancellationToken ct = default)
    {
        var result = await validator.ValidateAsync(instance, ct);
        if (result.IsValid) return true;

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        response.StatusCode  = 400;
        response.ContentType = "application/json";
        await response.WriteAsync(JsonSerializer.Serialize(new { errors }), ct);
        return false;
    }
}