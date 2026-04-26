using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(RanchoMvc.Startup))]

namespace RanchoMvc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
