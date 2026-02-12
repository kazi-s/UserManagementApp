namespace UserManagementApi.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? LastLoginTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RegistrationTime { get; set; }
    // We DON'T send PasswordHash or EmailConfirmationToken!
}