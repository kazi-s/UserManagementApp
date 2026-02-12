using UserManagementApi.Models;

namespace UserManagementApi.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}