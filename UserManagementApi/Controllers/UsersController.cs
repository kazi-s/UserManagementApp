using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.DTOs;
using UserManagementApi.Models;

namespace UserManagementApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .Where(u => u.Status != "deleted")
            .OrderByDescending(u => u.LastLoginTime)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                LastLoginTime = u.LastLoginTime,
                Status = u.Status,
                RegistrationTime = u.RegistrationTime
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost("block")]
    public async Task<IActionResult> BlockUsers(UserActionDto actionDto)
    {
        var users = await _context.Users
            .Where(u => actionDto.UserIds.Contains(u.Id))
            .ToListAsync();

        foreach (var user in users)
        {
            user.Status = "blocked";
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = $"{users.Count} user(s) blocked successfully" });
    }

    [HttpPost("unblock")]
    public async Task<IActionResult> UnblockUsers(UserActionDto actionDto)
    {
        var users = await _context.Users
            .Where(u => actionDto.UserIds.Contains(u.Id))
            .ToListAsync();

        foreach (var user in users)
        {
            user.Status = "active";
        }

        foreach (var user in users)
    {
        if (user.Status == "blocked")
        {
            user.Status = user.EmailConfirmationToken == null ? "active" : "unverified";
        }
    }

        await _context.SaveChangesAsync();
        return Ok(new { message = $"{users.Count} user(s) unblocked successfully" });
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteUsers(UserActionDto actionDto)
    {
    var users = await _context.Users
        .Where(u => actionDto.UserIds.Contains(u.Id))
        .ToListAsync();

    _context.Users.RemoveRange(users);
    await _context.SaveChangesAsync();
    
    return Ok(new { message = $"{users.Count} user(s) deleted permanently" });
    }

    [HttpPost("delete-unverified")]
    public async Task<IActionResult> DeleteUnverifiedUsers()
    {
        var unverifiedUsers = await _context.Users
            .Where(u => u.Status == "unverified")
            .ToListAsync();

        _context.Users.RemoveRange(unverifiedUsers);
        await _context.SaveChangesAsync();
        
        return Ok(new { message = $"{unverifiedUsers.Count} unverified user(s) deleted permanently" });
    }
}