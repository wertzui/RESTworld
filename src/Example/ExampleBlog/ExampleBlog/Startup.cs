using AutoMapper;
using ExampleBlog.Business;
using ExampleBlog.Common.Dtos;
using ExampleBlog.Data;
using ExampleBlog.Data.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExampleBlog
{
    public class Startup : RESTworld.AspNetCore.StartupBase
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
            // Add the database to the services and create an ODate context out of it which can be used for querying..
            services.AddDbContextFactoryWithDefaults<BlogDatabase>(Configuration);
            services.AddODataModelForDbContext<BlogDatabase>();
            services.MigrateDatabaseDuringStartup<BlogDatabase>();

            // Add a simple pipeline.
            services.AddRestPipeline<BlogDatabase, Blog, BlogDto, BlogDto, BlogDto, BlogDto>();

            // Add a pipeline which is versioned.
            // Have a look at the DTo, the AutomapperConfiguration and the database migration "AuthorV2" if you want to know more.
            services.AddRestPipeline<BlogDatabase, Author, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1>(new ApiVersion(1, 0), true);
            services.AddRestPipeline<BlogDatabase, Author, AuthorDto, AuthorDto, AuthorDto, AuthorDto>(new ApiVersion(2, 0));

            // We are using dedicated DTOs here to give the consumer a much better experience.
            // We add our custom authorization too. Get Post 42, or ony post with a valid HTTP status code as id to test it!
            services.AddRestPipelineWithAuthorization<BlogDatabase, Post, PostCreateDto, PostListDto, PostGetFullDto, PostUpdateDto, BlogpostAuthorizationHandler>(Configuration);

            services.AddScoped<MyCustomService>();
            services.AddScoped<MyCustomAuthorizationHandler>();

            base.ConfigureServices(services);
        }

        protected override void ConfigureAutomapper(IMapperConfigurationExpression config)
            => new AutoMapperConfiguration().ConfigureAutomapper(config);
    }
}