using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(HomeControlServer.Startup))]

namespace HomeControlServer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
