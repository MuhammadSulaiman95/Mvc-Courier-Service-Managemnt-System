using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MvcWebApiTask.Startup))]
namespace MvcWebApiTask
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
