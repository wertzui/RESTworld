using Asp.Versioning;
using ExampleBlog.Business;
using ExampleBlog.Business.Authorization;
using ExampleBlog.Business.Services;
using ExampleBlog.Business.Validation;
using ExampleBlog.Common.Dtos;
using ExampleBlog.Data;
using ExampleBlog.Data.Models;
using ExampleBlog.HalFormsCustomizations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RESTworld.EntityFrameworkCore.Models;
using System;


var builder = WebApplication.CreateBuilder(args);

var rwBuilder = builder.AddRestWorld();
var services = builder.Services;

services.AddAutoMapper(AutoMapperConfiguration.ConfigureAutomapper);

// Add the database to the services and create an OData context out of it which can be used for querying..
rwBuilder.AddDbContextFactoryWithDefaults<BlogDatabase>();
rwBuilder.AddODataModelForDbContext<BlogDatabase>();
rwBuilder.MigrateDatabaseDuringStartup<BlogDatabase>();

// Add a simple pipeline.
rwBuilder.AddCrudPipeline<BlogDatabase, Blog, BlogDto, BlogDto, BlogDto, BlogDto>();

// Add a pipeline which is versioned.
// Have a look at the DTo, the AutomapperConfiguration and the database migration "AuthorV2" if you want to know more.
rwBuilder.AddCrudPipeline<BlogDatabase, Author, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1>(new ApiVersion(1, 0), true);
rwBuilder.AddCrudPipeline<BlogDatabase, Author, AuthorDto, AuthorDto, AuthorDto, AuthorDto>(new ApiVersion(2, 0));

// We are using dedicated DTOs here to give the consumer a much better experience.
// We add our custom authorization too. Get Post 42, or any post with a valid HTTP status code as id to test it!
// We also add our own service to populate the Image and Attachment properties which do not come from the database.
rwBuilder.AddCrudPipelineWithCustomServiceAndAuthorization<BlogDatabase, Post, PostCreateDto, PostListDto, PostGetFullDto, PostUpdateDto, PostService, BlogpostAuthorizationHandler>();

// We also add some custom validation logic whenever a Post is created or updated
rwBuilder.AddValidator<PostValidatior, PostCreateDto, PostUpdateDto, Post>();

// Statistics can only be read, but not written so we use a read-only pipeline.
// Most times read-only will go hand in hand with a custom service.
rwBuilder.AddReadPipelineWithCustomService<BlogDatabase, Author, AuthorStatisticsListDto, AuthorStatisticsFullDto, AuthorStatisticsService>();

// For the MyCustomController we just need to add the service and the authorization handler,
// because ASP.Net Core automatically adds all Controllers
services.AddScoped<MyCustomService>();
services.AddScoped<MyCustomAuthorizationHandler>();
rwBuilder.AddFormsResourceGenerationCustomization<AuthorForPostCustomization>();

// The same goes for the deprecated V1
services.AddScoped<MyCustomServiceV1>();
services.AddScoped<MyCustomAuthorizationHandlerV1>();
rwBuilder.AddFormsResourceGenerationCustomization<AuthorForPostCustomizationV1>();

// A pipeline which uses a service that just generates random test data.
rwBuilder.AddCrudPipelineWithCustomService<BlogDatabase, ConcurrentEntityBase, TestDto, TestDto, TestDto, TestDto, TestService>();

// A simple pipeline that will return a users photo which can then be displayed in the frontend in the list views
// The PhotoController is automatically added, we only need to add the service.
services.AddHttpClient();
services.AddScoped<IPhotoService, PhotoService>();

var app = builder.Build();

app.UseRestWorld();

try
{
    await app.RunAsync();
}
catch (Exception e)
{
    Console.WriteLine(e);
}