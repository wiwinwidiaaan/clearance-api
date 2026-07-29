using Microsoft.AspNetCore.Identity;

namespace ClearanceAPI.Models;

// Extend IdentityUser bawaan ASP.NET Core untuk tambah field custom
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
