using Funq;
using ServiceStack.Razor;
using ServiceStack.WebHost.Endpoints;
using SiteDescriber.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SiteDescriber.Web
{
    public class AppHost : AppHostBase
    {
        public AppHost() : base("AGS Site Descriptor", typeof(GatewayService).Assembly) { }

        public override void Configure(Container container)
        {
            Plugins.Add(new RazorFormat());

            SetConfig(new EndpointHostConfig
            {
                GlobalHtmlErrorHttpHandler = new RazorHandler("/error")
            });
        }
    }
}