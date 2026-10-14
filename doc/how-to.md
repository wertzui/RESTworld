# How-To Cookbook

Task-oriented entry point for building an application on RESTworld. Each row points to the guide that explains the task and to the AI agent skill (in [`plugins/restworld/skills`](../plugins/restworld/skills)) that automates it. See [Using RESTworld with AI agents](ai-agents.md) to install them. The reference implementation for every task is [`src/Example/ExampleBlog`](../src/Example/ExampleBlog).

## Backend

| I want to… | Read | Agent skill |
|---|---|---|
| Create a new solution | [Getting Started](getting-started.md), [Health and Operations](health-and-operations.md) | `restworld-bootstrap-solution` |
| Add a complete new endpoint for an entity | [Getting Started](getting-started.md) | `restworld-add-entity-endpoint` |
| Define entities and the `DbContext`, run migrations | [Entities and Database](entities-and-database.md) | `restworld-create-entity-and-dbcontext` |
| Define the DTOs for an entity | [DTO Types and Roles](dto-types.md) | `restworld-create-dtos` |
| Map entities to DTOs | [Mapping and Versioning](mapping-and-versioning.md) | `restworld-create-mapper` |
| Decide between pipeline, custom service and custom controller | [Choosing a Pipeline](choosing_a_pipeline.md), [Pipeline Overview](pipeline-overview.md) | `restworld-choose-pipeline` |
| Add business logic or computed properties | [Services](services.md) | `restworld-custom-service` |
| Restrict who can read or change data | [Authorization](authorization.md) | `restworld-authorization-handler` |
| Validate create and update requests | [Validation](validation.md) | `restworld-validation` |
| Add an endpoint that is not plain CRUD | [Custom Controllers](custom-controllers.md) | `restworld-custom-controller` |
| Introduce a new API version | [Mapping and Versioning](mapping-and-versioning.md) | `restworld-add-api-version` |
| Change the generated HAL-FORMS templates | [Forms Customization](forms-customization.md), [DTO Types and Roles](dto-types.md) | `restworld-halforms-customization` |
| Use Swagger, MCP, CSV, JSON Patch, caching or history | [API Extras](api-extras.md) | `restworld-api-extras` |
| Test services, mappers and handlers | [Testing](testing.md) | `restworld-testing` |

## Clients

| I want to… | Read | Agent skill |
|---|---|---|
| Call a RESTworld API from .NET | [.NET Client Development](client-development-dotnet.md) | `restworld-dotnet-client` |
| Add a list or edit page to the Angular app | [Angular Client Development](client-development-angular.md) | `restworld-angular-add-page` |
| Customize avatars, form layout, theme or inputs | [Angular Client Components](client-development-angular-components.md) | `restworld-angular-custom-components` |

## Typical order for a new endpoint

1. Entity and `DbContext`, then a migration.
2. DTOs.
3. Mapper.
4. Pipeline registration, plus an optional service, authorization handler and validator.
5. Check the result in HAL-Explorer or Swagger.
6. Angular route and menu entry (optional).
