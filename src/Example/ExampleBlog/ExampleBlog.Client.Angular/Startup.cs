using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExampleBlog.Client.Angular
{
    public class Startup : RESTworld.Client.AspNetCore.StartupBase
    {
        public Startup(IConfiguration configuration)
            : base(configuration)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            base.Configure(app, env, provider);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
        }
    }
}