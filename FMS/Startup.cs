using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Autofac;

[assembly: OwinStartup(typeof(FMS.Startup))]

namespace FMS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            ConfigureFilters(app, config);
            ConfigureWebApi(app, config);

            var container = ConfigureDI(app, config);

            app.UseAutofacWebApi(config);
            app.UseAutofacMiddleware(container);


            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions()
            {
                Provider = container.Resolve<IOAuthBearerAuthenticationProvider>()
            });

            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/api/auth/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(7),
                Provider = container.Resolve<IOAuthAuthorizationServerProvider>()
            });

            app.UseWebApi(config);
        }
    }
}
