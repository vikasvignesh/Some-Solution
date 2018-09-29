using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TextBoxs.Startup))]
namespace TextBoxs
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
