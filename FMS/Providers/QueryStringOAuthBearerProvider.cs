using Microsoft.Owin.Security.OAuth;
using System.Threading.Tasks;

namespace FMS.Providers
{
    public class QueryStringOAuthBearerProvider : OAuthBearerAuthenticationProvider
    {
        private readonly string _name;

        public QueryStringOAuthBearerProvider()
        {
            _name = "sid";
        }

        public QueryStringOAuthBearerProvider(string name)
        {
            _name = name;
        }

        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            var qvalue = context.Request.Query.Get(_name);
            var hvalue = context.Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(qvalue))
            {
                context.Token = qvalue;
            }
            else if (!string.IsNullOrEmpty(hvalue))
            {
                context.Token = hvalue;
            }

            return Task.FromResult<object>(null);
        }
    }
}
