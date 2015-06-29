using Domain.Concrete;
using Domain.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Domain.Concreate
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(ApplicationDbContext context) : base(new UserStore<ApplicationUser>(context))
        {

        }
    }


}
