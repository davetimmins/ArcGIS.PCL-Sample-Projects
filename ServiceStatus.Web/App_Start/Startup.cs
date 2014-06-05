using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ServiceStatus.Web.App_Start.Startup))]
namespace ServiceStatus.Web.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();

            ArcGIS.ServiceModel.Serializers.JsonDotNetSerializer.Init();
        }
    }
} 
