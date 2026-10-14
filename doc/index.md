# RESTworld Documentation

Welcome to the RESTworld documentation. These guides help you build and operate applications with the RESTworld framework.

- [Getting Started](getting-started.md) – scaffold your solution, wire RESTworld services, and spin up your first API and optional Angular frontend.
- [Pipeline Overview](pipeline-overview.md) – understand the default request flow and learn when to customize it, with a link to the [Choosing a Pipeline guide](choosing_a_pipeline.md).
- [Configuration](configuration.md) – configure RESTworld in `appsettings.json` for both APIs and clients.
- [DTO Types and Roles](dto-types.md) – understand the six DTO types, the special role of `TQueryDto` in OData queries, and how navigation properties drive HAL-Forms foreign-key options.
- [Using RESTworld with AI agents](ai-agents.md) – install the RESTworld skills as a Copilot plugin or copy them into your repository.
- [How-To Cookbook](how-to.md) – find the guide (and the matching AI agent skill) for a concrete task.
- [Entities and Database](entities-and-database.md) – entity base classes, the DbContext, temporal tables and migrations.
- [Mapping and Versioning](mapping-and-versioning.md) – set up Mapperly mappers (and migrate legacy AutoMapper code) and manage media-type versioning strategies.
- [Services](services.md) – add business logic and computed properties with custom services.
- [Authorization](authorization.md) – implement granular authorization handlers using RESTworld abstractions.
- [Validation](validation.md) – validate create and update requests.
- [Custom Controllers](custom-controllers.md) – add endpoints that do not fit the CRUD pipeline.
- [Forms Customization](forms-customization.md) – adjust the generated HAL-FORMS templates.
- [API Extras](api-extras.md) – Swagger, MCP, CSV, JSON Patch, caching, history and concurrency.
- [Testing](testing.md) – test services, mappers and handlers with RESTworld.Testing.
- [Health and Operations](health-and-operations.md) – expose health probes, integrate Aspire, and enable reliable local testing.
- [Angular Client Development](client-development-angular.md) – build SPAs with the RESTworld Angular client.
- [.NET Client Development](client-development-dotnet.md) – consume RESTworld APIs from .NET apps and services.

Looking for quick decision support on pipeline customization? Jump straight to [Choosing a Pipeline](choosing_a_pipeline.md).
