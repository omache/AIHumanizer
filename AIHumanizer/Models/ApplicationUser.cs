using Microsoft.AspNetCore.Identity;

namespace AIHumanizer.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool isSubscribed { get; set; } = false;
    }
}
