# Pipeline Overview

RESTworld orchestrates a rich request pipeline so you can focus on business rules instead of plumbing. Understanding the core flow helps you decide when to plug in custom services, validators, or controllers.

## Default request flow

For list endpoints, the default pipeline runs through these stages:

1. **Request** arrives via ASP.NET Core routing.
2. **Controller selection** resolves based on route and (optional) media-type versioning.
3. **Query parsing** uses OData to translate query options into LINQ expressions.
4. **Controller** delegates work to the business service.
5. **Authorization (pre-processing)** can validate or adjust the request.
6. **Data access** loads entities via Entity Framework Core.
7. **Mapping** converts entities to DTOs through AutoMapper.
8. **Authorization (post-processing)** can inspect or modify the response.
9. **HAL response building** wraps the payload with links and metadata.
10. **Result** is serialized and returned to the client.

Use this flow as a reference when deciding where to inject custom logic—validators, authorization handlers, or service overrides.

## Choosing the right pipeline

RESTworld offers multiple pipeline templates (read-only, CRUD, custom services, custom controllers). Use the decision flow in [Choosing a Pipeline](choosing_a_pipeline.md) to select the best fit for your endpoint. Each option ties back to the extension methods listed in [Getting Started](getting-started.md).

## Extending the pipeline

- **Validators:** Run bespoke validation before or after saving changes. See examples in [Getting Started](getting-started.md).
- **Custom services:** Override the default read or CRUD services when you need bespoke data access. Review the decision points in [Choosing a Pipeline](choosing_a_pipeline.md).
- **Custom controllers:** When your endpoint does not map cleanly to the built-in pipeline, inherit from `RestControllerBase`, `ReadControllerBase`, or `CrudControllerBase` to retain HAL support while owning the logic.

Next, configure runtime behavior in [Configuration](configuration.md) and register authorization handlers in [Authorization](authorization.md).
