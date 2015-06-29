using Microsoft.AspNet.Identity.EntityFramework;

namespace Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
