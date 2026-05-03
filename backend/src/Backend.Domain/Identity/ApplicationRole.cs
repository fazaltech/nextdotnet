using Microsoft.AspNetCore.Identity;

namespace Backend.Domain.Identity;

public sealed class ApplicationRole : IdentityRole<int>
{
    public string? Description { get; set; }
}
