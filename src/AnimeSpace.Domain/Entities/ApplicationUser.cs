using Microsoft.AspNetCore.Identity;

namespace AnimeSpace.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
