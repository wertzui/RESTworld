# Mapping and Versioning

RESTworld leans on AutoMapper and media-type negotiation to keep APIs evolvable. This guide shows how to organize profiles and support multiple DTO versions.

## AutoMapper configuration

Create an `AutoMapperConfiguration` class in your `MyApi.Business` project and expose a method that RESTworld can register:

```csharp
using AutoMapper;
using MyApi.Common.Dtos;
using MyApi.Common.Enums;
using MyApi.Data.Models;

namespace MyApi.Business;

public static class AutoMapperConfiguration
{
    public static void ConfigureAutomapper(IMapperConfigurationExpression config)
    {
        // Simple bidirectional mapping
        config
            .CreateMap<TEntity, TDto>()
            .ReverseMap();

        // Add mappings for each version as needed
        config
            .CreateMap<MyEntity, MyDto>()
            .ReverseMap();

        config
            .CreateMap<MyEntity, MyDtoV1>()
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
            .ReverseMap()
            .ForMember(dst => dst.FirstName, opt => opt.MapFrom(src => src.Name.Split(new[] { ' ' }, 2)[0]))
            .ForMember(dst => dst.LastName, opt => opt.MapFrom(src => src.Name.Split(new[] { ' ' }, 2)[1]));

        // Add more mappings as your domain evolves
    }
}
```

Register the profile in your `Program.cs` (`builder.Services.AddAutoMapper(AutoMapperConfiguration.ConfigureAutomapper);`). See [Getting Started](getting-started.md) for the full bootstrapping sample.

## Versioning strategy

RESTworld parses API versions from media-type headers and echoes them back so clients can adapt. Configure defaults in [Configuration](configuration.md) and align your DTO mappings accordingly.

- **Preferred approach:** keep all version-specific logic in AutoMapper to avoid branching inside services. This way you can just create two Read- or CRUD pipelines which only differ in the Version and the DTO.
- **Alternative:** create version-specific services and pipelines when behavior diverges beyond simple mapping. Use the extension methods shown in [Getting Started](getting-started.md) and consult [Choosing a Pipeline](choosing_a_pipeline.md).

### Accept header format

Clients should request specific versions via the `Accept` header, for example: `application/hal+json; v=42`. RESTworld replies with the negotiated version in the `Content-Type` header and advertises supported/deprecated versions through `api-supported-versions` and `api-deprecated-versions` response headers.

### Handling deprecated versions

Monitor the `api-deprecated-versions` header in your client implementations and surface warnings proactively. Plan for version transitions by maintaining parallel DTOs and mappings until consumers migrate.

Continue with [Authorization](authorization.md) to secure endpoints or review [Angular Client Development](client-development-angular.md) and [.NET Client Development](client-development-dotnet.md) for version-aware client patterns.
