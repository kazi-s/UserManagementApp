using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.DTOs;
using UserManagementApi.Models;
using UserManagementApi.Services;
using BCrypt.Net;

namespace UserManagementApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthController(
        ApplicationDbContext context, 
        IJwtService jwtService,
        IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    [HttpPost("register")]
public async Task<IActionResult> Register(RegisterDto registerDto)
{
    try
    {
        var user = new User
        {
            Name = registerDto.Name,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            RegistrationTime = DateTime.UtcNow,
            Status = "unverified",
            EmailConfirmationToken = Guid.NewGuid().ToString(),
            EmailConfirmationTokenExpires = DateTime.UtcNow.AddHours(24),
            IsEmailConfirmed = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _ = Task.Run(async () =>
        {
            try
            {
                var emailService = HttpContext.RequestServices.GetRequiredService<IEmailService>();
                await emailService.SendConfirmationEmailAsync(user.Email, user.Name, user.EmailConfirmationToken);
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AuthController>>();
                logger.LogError(ex, $"Failed to send confirmation email to {user.Email}");
            }
        });

        return Ok(new { message = "Registration successful! Please check your email to confirm your account." });
    }
    catch (DbUpdateException ex) 
    when (ex.InnerException?.Message.Contains("unique constraint") == true 
          || ex.InnerException?.Message.Contains("IX_Users_Email_Unique") == true)
    {
        return Conflict(new { message = "Email already exists" });
    }
}

    [HttpGet("confirm-email")]
public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    
    if (user == null)
    {
        return NotFound(new { message = "User not found." });
    }

    if (user.IsEmailConfirmed)
    {
        return Ok(new { message = "Email already confirmed." });
    }

    if (user.EmailConfirmationToken != token || 
        user.EmailConfirmationTokenExpires < DateTime.UtcNow)
    {
        return BadRequest(new { message = "Invalid or expired confirmation link." });
    }

    user.IsEmailConfirmed = true;
    user.Status = "active";  // Unverified → Active
    user.EmailConfirmationToken = null;
    user.EmailConfirmationTokenExpires = null;

    await _context.SaveChangesAsync();

    return Ok(new { message = "Email confirmed successfully! You can now login." });
}

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
        
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password" });

        if (user.Status == "blocked")
            return Unauthorized(new { message = "Your account is blocked" });

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password" });

        user.LastLoginTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            Status = user.Status
        });
    }
}