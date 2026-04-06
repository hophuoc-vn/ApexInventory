using Apex.Domain.Common;

namespace Apex.Domain.Entities;

public class User : BaseEntity
{
    public User()
    {
        Id = Guid.NewGuid();
    }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // "Admin" or "User"
}