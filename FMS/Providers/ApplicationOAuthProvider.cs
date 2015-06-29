using Autofac;
using Autofac.Integration.Owin;
using Domain.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.OAuth;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FMS.Providers
{

    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var scope = context.OwinContext.GetAutofacLifetimeScope();
            using (var _repo = scope.Resolve<UserManager<ApplicationUser>>())
            {
                var user = await _repo.FindAsync(context.UserName, context.Password);

                if (user == null)
                {
                    context.SetError("The user name or password is incorrect.");
                    return;
                }

                var roles = await _repo.GetRolesAsync(user.Id);

                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                foreach (var role in roles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
                context.Validated(identity);
            }
        }
    }
}