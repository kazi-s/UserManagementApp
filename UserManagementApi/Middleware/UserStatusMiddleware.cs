using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserManagementApi.Data;
using System.IdentityModel.Tokens.Jwt;


namespace UserManagementApi.Middleware;

public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserStatusMiddleware> _logger;

    public UserStatusMiddleware(RequestDelegate next, ILogger<UserStatusMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        var path = context.Request.Path.ToString().ToLower();
        if (path.Contains("/auth/login") || path.Contains("/auth/register"))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                            ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            
            if (int.TryParse(userIdClaim, out var userId))
            {
                var user = await dbContext.Users.FindAsync(userId);
                
                if (user == null)
                {
                    _logger.LogWarning($"User {userId} not found in database");
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { message = "ERROR: User not found. Please login again." });
                    return;
                }

                if (user.Status == "blocked")
                {
                    _logger.LogWarning($"User {userId} is blocked");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { message = "ERROR: Your account has been blocked." });
                    return;
                }
            }
        }

        await _next(context);
    }
}