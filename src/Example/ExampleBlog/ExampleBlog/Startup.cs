using AutoMapper;
using ExampleBlog.Business;
using ExampleBlog.Common.Dtos;
using ExampleBlog.Data;
using ExampleBlog.Data.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            base.Configure(app, env);

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public override void ConfigureServices(IServiceCollection services)
        {
            // Add the database to the services and create an ODate context out of it which can be used for querying..
            services.AddDbContextFactoryWithDefaults<BlogDatabase>(Configuration);
            services.AddODataModelForDbContext<BlogDatabase>();
            services.MigrateDatabaseDuringStartup<BlogDatabase>();

            // Add some simple pipelines.
            services.AddRestPipeline<BlogDatabase, Blog, BlogDto, BlogDto, BlogDto, BlogDto>();
            services.AddRestPipeline<BlogDatabase, Author, AuthorDto, AuthorDto, AuthorDto, AuthorDto>();

            // We are using dedicated DTOs here to give the consumer a much better experience.
            services.AddRestPipeline<BlogDatabase, Post, PostCreateDto, PostListDto, PostGetFullDto, PostUpdateDto>();

            base.ConfigureServices(services);
        }

        protected override void ConfigureAutomapper(IMapperConfigurationExpression config)
            => new AutoMapperConfiguration().ConfigureAutomapper(config);
    }
}
