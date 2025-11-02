# RESTworld

<!-- markdownlint-disable-next-line MD033 -->
<img src="logo/RESTworld.svg" alt="RESTworld logo" width="128" />

RESTworld is a developer-focused framework that turns ASP.NET Core and Entity Framework Core into a fully RESTful, HAL-compliant platform. It combines well-known building blocks with batteries-included conventions so you can deliver production-ready APIs quickly.

## Key capabilities

- Opinionated pipelines for read and CRUD scenarios, with extensibility hooks when you need to customize.
- Out-of-the-box support for HAL, HAL-FORMS, and OData (query, paging, templated links).
- AutoMapper, resource-based authorization, and API versioning baked into the request flow.
- Telemetry via OpenTelemetry, health endpoints for readiness/liveness/startup, and Aspire integration for local orchestration.

## Getting started

1. Follow the [Getting Started](doc/getting-started.md) guide to scaffold your solution and wire up RESTworld services.
2. Configure runtime behavior with [Configuration](doc/configuration.md), and review [Pipeline Overview](doc/pipeline-overview.md) to choose the right pipeline.
3. Secure your endpoints using [Authorization](doc/authorization.md) and fine-tune DTO mappings with [Mapping and Versioning](doc/mapping-and-versioning.md).

Need a reference implementation? Explore [`src/Example/ExampleBlog`](src/Example/ExampleBlog).

## Documentation map

- [RESTworld documentation index](doc/index.md)
- [Choosing a Pipeline](doc/choosing_a_pipeline.md)
- [Health and Operations](doc/health-and-operations.md)
- [Angular Client Development](doc/client-development-angular.md)
- [.NET Client Development](doc/client-development-dotnet.md)

The documentation targets developers who build APIs and client applications on top of the RESTworld libraries. If you're looking for a higher-level pitch, read the refreshed [README-short](README-short.md).

## Contributing and feedback

- File issues or feature requests via [GitHub Issues](https://github.com/wertzui/RESTworld/issues).
- Share ideas or improvements through pull requests—guided by the documentation above.

Enjoy building RESTful applications with RESTworld!
