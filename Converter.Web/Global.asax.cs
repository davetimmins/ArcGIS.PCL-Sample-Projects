using Converter.Web.Interface;
using Funq;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.Razor;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace Converter.Web
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            new AppHost().Init();
        }

        protected void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");
        }

        protected void Application_BeginRequest(object src, EventArgs e)
        {
            if (Request.IsLocal)
                ServiceStack.MiniProfiler.Profiler.Start();
        }

        protected void Application_EndRequest(object src, EventArgs e)
        {
            ServiceStack.MiniProfiler.Profiler.Stop();
        }
    }

    public class AppHost : AppHostBase
    {
        public AppHost() : base("Converter", typeof(ConverterService).Assembly) { }

        public override void Configure(Container container)
        {
            //Register In-Memory Cache provider. 
            container.Register<ICacheClient>(new MemoryCacheClient());

            Plugins.Add(new RazorFormat());
            
            SetConfig(new EndpointHostConfig
            {
                GlobalResponseHeaders = new Dictionary<String, String>()
            });

            JsConfig.EmitCamelCaseNames = true;
            JsConfig.IncludeTypeInfo = false;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            JsConfig.IncludeNullValues = false;
        }
    }   
}