using Funq;
using ServiceStack.Razor;
using ServiceStack.WebHost.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ServiceStack.Text;
using System.Diagnostics;
using SiteDescriber.ServiceInterface;

namespace SiteDescriber.Console
{
    public class AppHost : AppHostHttpListenerBase
    {
        public AppHost() : base("AGS Site Descriptor", typeof(GatewayService).Assembly) { }

        public override void Configure(Container container)
        {
            Plugins.Add(new RazorFormat());                      

            SetConfig(new EndpointHostConfig
            {
                GlobalHtmlErrorHttpHandler =  new RazorHandler("/error") 
            });
        }

        //Run it!
        static void Main(string[] args)
        {
            var listeningOn = args.Length == 0 ? "http://*:1337/" : args[0];
            var appHost = new AppHost();
            appHost.Init();
            appHost.Start(listeningOn);

            System.Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);
            "Type Ctrl+C to quit..".Print();

            var ieProcess = new Process();
            // Set the application that our Process is
            // going to start.
            ieProcess.StartInfo.FileName = "iexplore";
            // Pass in the url that iexplore will visit upon
            // starting as a command line argument.
            ieProcess.StartInfo.Arguments = "http://localhost:1337/";
            // Start the command line execution
            ieProcess.Start();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
