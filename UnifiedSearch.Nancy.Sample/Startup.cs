using Nancy;
using Nancy.Diagnostics;
using Owin;
using UnifiedSearch.Nancy.Interface;

public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        app.UseNancy();
    }

    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(Nancy.TinyIoc.TinyIoCContainer container)
        {            
            base.ConfigureApplicationContainer(container);

            container.Register<SearchModule, SearchModule>().AsSingleton();
        }
    }
}