using Funq;
using ServiceStack;
using ServiceStack.Razor;
using SiteDescriber.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace SiteDescriber.Web
{
    public class AppHost : AppHostBase
    {
        public AppHost() : base("AGS Site Descriptor", typeof(GatewayService).Assembly) { }

        public override void Configure(Container container)
        {
            Plugins.Add(new RazorFormat());

            Plugins.RemoveAll(x => x is RequestInfoFeature);
            Plugins.RemoveAll(x => x is MetadataFeature);
            Plugins.RemoveAll(x => x is PredefinedRoutesFeature);
                        
            CustomErrorHttpHandlers.Clear();
            CustomErrorHttpHandlers.Add(HttpStatusCode.InternalServerError, new RazorHandler("/error"));

            SetConfig(new HostConfig
            {
                GlobalResponseHeaders = new Dictionary<String, String>()
            });
        }
    }
}