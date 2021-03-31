# RESTworld

RESTworld is a framework which utilizes other common frameworks and patterns alltogether to enable easy and fast creation of a truly RESTful API.

## Used frameworks and patterns
- Entity Framework Core for data access
- ASP.Net Core for hosting
- HAL for providing hyperlinks between resources
- OData for query support on list endpoints
- AutoMapper for mapping between Entities and DTOs

## Pipeline
The most basic pipeline has the following data flow for a request on a list endpoint:

1. Request
2. Controller selection through ASP.Net Core
3. Query parsing through OData
4. Controller method calls business service method
5. Service gets the data through Entity Framework Core
6. Entity Framework Core translates the query into SQL and gets the data from the database
7. Business service translates Entities into DTOs through Automapper
8. Controller wraps the result in a HAL response
9. Result

## Usage
### Solution structure
If your API gets the name MyApi, structure your Solution with the following Projects:
- MyApi (ASP.Net Core Web API)
  - References RESTworld.AspNetCore, MyApi.Business
  - Contains your startup logic and your custom controllers
- MyApi.Business
  - References RESTworld.Business, MyApi.Data
  - Contains your AutoMapperConfiguration and your custom services
- MyApi.Data
  - References RESTworld.EntityFrameworkCore, MyApi.Common
  - Contains your Entity Framework Core Database Model including Entities and Migrations
- MyApi.Common
  - References RESTworld.Common
  - Contains your DTOs and Enums

### Startup configuration
Add the following to your appsettings.json
```
"RESTworld": {
  "MaxNumberForListEndpoint": <whatever is an appropriate number of resources for one page>
}
```

Change your Program.cs to the following
```
namespace MyApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RESTworld.AspNetCore.Program<Startup>.Main(args);
        }
    }
}
```

Change or add your Startup class
```
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RESTworld.Business.Abstractions;
using MyApi.Common.Dtos;
using MyApi.Data;
using MyApi.Data.Models;
using MyApi.Business;

namespace MyApi
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

            // Optionally migrate your database to the latest version during startup
            MigrateDatabase<TDbContext>(app);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public override void ConfigureServices(IServiceCollection services)
        {
            // Database
            services.AddDbContextFactoryWithDefaults<MyDatabase>(Configuration);
            services.AddODataModelForDbContext<MyDatabase>();
            
            // Default pipeline
            services.AddRestPipeline<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();

            // With custom service
            services.AddRestPipelineWithCustomService<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TService>();

            // With custom controller
            services.AddRestPipelineWithCustomController<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TController>();

            base.ConfigureServices(services);
        }

        protected override void ConfigureAutomapper(IMapperConfigurationExpression config)
            => new AutoMapperConfiguration().ConfigureAutomapper(config);
    }
}
```

Add an AutoMapperConfiguration to your MyApi.Business project
```
using AutoMapper;
using MyApi.Common.Dtos;
using MyApi.Common.Enums;
using MyApi.Data.Models;

namespace MyApi.Business
{
    public class AutoMapperConfiguration
    {
        public void ConfigureAutomapper(IMapperConfigurationExpression config)
        {
            config.CreateMap<TEntity, TDto>();

            // Add more mappings
        }
    }
}
```

That's it. Now you can start your API and use a HAL browser like https://chatty42.herokuapp.com/hal-explorer/index.html#uri=https://localhost:5001 to browse your API.
If you are using a `launchSettings.json`, I suggest to use this as your `"launchUrl"`.