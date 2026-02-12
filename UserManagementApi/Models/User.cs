using System;

namespace UserManagementApi.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime RegistrationTime { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public string Status { get; set; } = "unverified";
    public string? EmailConfirmationToken { get; set; }
    public bool IsEmailConfirmed { get; set; } = false; 
    public DateTime? EmailConfirmationTokenExpires { get; set; } 
}