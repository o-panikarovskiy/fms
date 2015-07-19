using Autofac;
using Autofac.Integration.WebApi;
using Domain.Abstract;
using Domain.Concreate;
using Domain.Concrete;
using Domain.Models;
using FMS.Providers;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System.Reflection;
using System.Web.Http;

namespace FMS
{
    public partial class Startup
    {
        public IContainer ConfigureDI(IAppBuilder app, HttpConfiguration config)
        {
            var builder = new ContainerBuilder();

            //Repositories          
            builder.RegisterType<ApplicationDbContext>().As<ApplicationDbContext>().InstancePerRequest();
            builder.RegisterType<ApplicationUserManager>().As<UserManager<ApplicationUser>>().InstancePerRequest();
            builder.RegisterType<ApplicationRepository<UploadingProgress>>().As<IRepository<UploadingProgress>>().InstancePerRequest();
            builder.RegisterType<ApplicationRepository<SearchQuery>>().As<IRepository<SearchQuery>>().InstancePerRequest();
            builder.RegisterType<CSVImport>().As<IFileImport>().InstancePerRequest();

            builder.RegisterType<ApplicationRepository<Person>>().As<IRepository<Person>>().InstancePerRequest();            
            builder.RegisterType<ApplicationRepository<PersonFact>>().As<IRepository<PersonFact>>().InstancePerRequest();            
            builder.RegisterType<ApplicationRepository<PersonParameter>>().As<IRepository<PersonParameter>>().InstancePerRequest();

            builder.RegisterType<ApplicationRepository<Document>>().As<IRepository<Document>>().InstancePerRequest();
            builder.RegisterType<ApplicationRepository<DocumentParameter>>().As<IRepository<DocumentParameter>>().InstancePerRequest();            
            builder.RegisterType<ApplicationRepository<DocumentFact>>().As<IRepository<DocumentFact>>().InstancePerRequest();            

            builder.RegisterType<ApplicationRepository<ParameterName>>().As<IRepository<ParameterName>>().InstancePerRequest();

            builder.RegisterType<ApplicationRepository<Misc>>().As<IRepository<Misc>>().InstancePerRequest();
            builder.RegisterType<ApplicationRepository<MiscName>>().As<IRepository<MiscName>>().InstancePerRequest();
            

            ////Authorization            
            builder.RegisterType<ApplicationOAuthProvider>().As<IOAuthAuthorizationServerProvider>();
            builder.RegisterType<QueryStringOAuthBearerProvider>().As<IOAuthBearerAuthenticationProvider>();

            // Register your Web API controllers.
            builder.RegisterWebApiFilterProvider(config);
            builder.RegisterWebApiModelBinders(Assembly.GetExecutingAssembly());
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            return container;
        }
    }
}
