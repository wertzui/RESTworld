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
using RESTworld.EntityFrameworkCore.Models;

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
            // Add the database to the services and create an OData context out of it which can be used for querying..
            services.AddDbContextFactoryWithDefaults<BlogDatabase>(Configuration);
            services.AddODataModelForDbContext<BlogDatabase>();
            services.MigrateDatabaseDuringStartup<BlogDatabase>();

            // Add a simple pipeline.
            services.AddCrudPipeline<BlogDatabase, Blog, BlogDto, BlogDto, BlogDto, BlogDto>();

            // Add a pipeline which is versioned.
            // Have a look at the DTo, the AutomapperConfiguration and the database migration "AuthorV2" if you want to know more.
            services.AddCrudPipeline<BlogDatabase, Author, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1>(new ApiVersion(1, 0), true);
            services.AddCrudPipeline<BlogDatabase, Author, AuthorDto, AuthorDto, AuthorDto, AuthorDto>(new ApiVersion(2, 0));

            // We are using dedicated DTOs here to give the consumer a much better experience.
            // We add our custom authorization too. Get Post 42, or any post with a valid HTTP status code as id to test it!
            // We also add our own service to populate the Image and Attachement properties which do not come from the database.
            services.AddCrudPipelineWithCustomServiceAndAuthorization<BlogDatabase, Post, PostCreateDto, PostListDto, PostGetFullDto, PostUpdateDto, PostService, BlogpostAuthorizationHandler>(Configuration);

            // For the MyCustomController we just need to add the service and the authorization handler,
            // because ASP.Net Core automatically adds all Controllers
            services.AddScoped<MyCustomService>();
            services.AddScoped<MyCustomAuthorizationHandler>();

            // The same goes for the deprecated V1
            services.AddScoped<MyCustomServiceV1>();
            services.AddScoped<MyCustomAuthorizationHandlerV1>();

            // A pipeline which uses a service that just generates random test data.
            services.AddCrudPipelineWithCustomService<BlogDatabase, ConcurrentEntityBase, TestDto, TestDto, TestDto, TestDto, TestService>();

            base.ConfigureServices(services);
        }

        protected override void ConfigureAutomapper(IMapperConfigurationExpression config)
            => new AutoMapperConfiguration().ConfigureAutomapper(config);
    }
}