using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebAppWithASPdotNETIdentity.Startup))]
namespace WebAppWithASPdotNETIdentity
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
