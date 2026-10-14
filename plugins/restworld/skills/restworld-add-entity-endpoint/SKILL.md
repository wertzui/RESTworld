---
name: restworld-add-entity-endpoint
description: 'End-to-end checklist for adding a new REST endpoint for a database entity to a RESTworld API. Use when asked to add an endpoint, add a new resource, expose an entity through the API, add CRUD for a table, add a new controller for an entity, scaffold a complete feature (entity, migration, DTOs, mapper, pipeline registration), or when unsure which RESTworld steps and skills to apply in which order. Orchestrates restworld-create-entity-and-dbcontext, restworld-create-dtos, restworld-create-mapper, restworld-choose-pipeline and the optional service, authorization, validation and Angular page skills, and explains how to verify the result by building and calling the API (HAL-Explorer, Swagger).'
argument-hint: 'Describe the resource to expose (entity name, properties, relations, read-only or CRUD, API version, special rules such as authorization or validation, whether an Angular page is wanted).'
---

# RESTworld: Add an Entity Endpoint (Orchestrator)

## When to Use
- You want a new resource served by the API and it is (or will be) backed by a database table
- You want an ordered to-do list that references the detailed skills instead of repeating them

This skill only orders the work. Details live in the linked skills.

## Prerequisite: Know the Solution Layout
`MyApi` (host, `Program.cs`, custom controllers) -> `MyApi.Business` (mappers, services, authorization, validation) -> `MyApi.Data` (entities, DbContext, migrations) -> `MyApi.Common` (DTOs, enums). New solution? Use [restworld-bootstrap-solution](../restworld-bootstrap-solution/SKILL.md) first.

## Ordered Checklist

| # | Step | Where | Skill / doc |
|---|---|---|---|
| 1 | Entity class (`ChangeTrackingEntityBase` for CRUD) + `DbSet<T>` on the `DbContextBase` | `MyApi.Data/Models`, `MyApi.Data/<Context>.cs` | [restworld-create-entity-and-dbcontext](../restworld-create-entity-and-dbcontext/SKILL.md) |
| 2 | EF Core migration (`dotnet ef migrations add ...`) | `MyApi.Data/Migrations` | same skill, step 7 |
| 3 | DTOs: `CreateDto`, `QueryDto`, `GetListDto`, `GetFullDto`, `UpdateDto` (+ enums) | `MyApi.Common/Dtos`, `MyApi.Common/Enums` | [restworld-create-dtos](../restworld-create-dtos/SKILL.md) |
| 4 | Mapper (Mapperly) | `MyApi.Business/Mapping` | [restworld-create-mapper](../restworld-create-mapper/SKILL.md) |
| 5 | Decide the pipeline (plain / custom service / custom controller; versioned or not) | decision only | [restworld-choose-pipeline](../restworld-choose-pipeline/SKILL.md) |
| 6 | Register pipeline and mapper | `MyApi/Program.cs` | step 6 below |
| 7 | *Optional* custom service (FK data, computed fields, extra logic) | `MyApi.Business/Services` | [restworld-custom-service](../restworld-custom-service/SKILL.md) |
| 8 | *Optional* authorization handler | `MyApi.Business/Authorization` | [restworld-authorization-handler](../restworld-authorization-handler/SKILL.md) |
| 9 | *Optional* validation | `MyApi.Business/Validation` | [restworld-validation](../restworld-validation/SKILL.md) |
| 10 | *Optional* extra routes / non-DB endpoint | `MyApi/Controllers` | [restworld-custom-controller](../restworld-custom-controller/SKILL.md) |
| 11 | *Optional* HAL-Forms tweaks (dropdowns, hidden fields) | `MyApi/HalFormsCustomizations` | [restworld-halforms-customization](../restworld-halforms-customization/SKILL.md) |
| 12 | Build and call the API | terminal | step 12 below |
| 13 | *Optional* tests | test project | [restworld-testing](../restworld-testing/SKILL.md) |
| 14 | *Optional* Angular list/edit page | Angular project | [restworld-angular-add-page](../restworld-angular-add-page/SKILL.md) |

Do the steps in order; each later step compiles against the earlier ones. Steps 7-9 can be added later without touching the earlier ones, except that the registration in step 6 changes (see below).

## Step 6: Registration in `Program.cs`
Pick the row that matches the decision from step 5 (all are `IHostApplicationBuilder` extensions on the builder returned by `AddRestWorld()`):

```csharp
var rwBuilder = builder.AddRestWorld();
rwBuilder.AddDbContextFactoryWithDefaults<BlogDatabase>();   // once per DbContext

// Plain CRUD
rwBuilder.AddCrudPipeline<BlogDatabase, Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto>();
rwBuilder.AddCrudMapper<Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto, BlogMapper>();

// CRUD + custom service + authorization (Post in ExampleBlog; it has FKs to Blog and Author)
rwBuilder.AddCrudPipelineWithCustomServiceAndAuthorization<BlogDatabase, Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto, PostService, BlogpostAuthorizationHandler>();
rwBuilder.Services.AddCrudMapper<Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto, PostMapper>();

// Validation (create + update)
rwBuilder.AddValidator<PostValidatior, PostCreateDto, PostUpdateDto, Post>();

// Read-only + custom service
rwBuilder.AddReadPipelineWithCustomService<BlogDatabase, Author, AuthorStatisticsListDto, AuthorStatisticsListDto, AuthorStatisticsFullDto, AuthorStatisticsService>();
rwBuilder.AddReadMapper<Author, AuthorStatisticsListDto, AuthorStatisticsListDto, AuthorStatisticsFullDto, YourReadMapper>(); // a ReadMapperlyMapperBase subclass (ExampleBlog still uses the legacy AddReadAutoMapper here on purpose, to show the migration path)
```
Available pipeline methods: `AddCrudPipeline`, `AddCrudPipelineWithAuthorization`, `AddCrudPipelineWithCustomService`, `AddCrudPipelineWithCustomServiceAndAuthorization`, and the same four with `Read`. Only the plain and `...WithAuthorization` variants take `ApiVersion? apiVersion = null, bool isDeprecated = false`; the `...WithCustomService...` variants have no version parameter (versioning rules: choose-pipeline Rule B and [restworld-add-api-version](../restworld-add-api-version/SKILL.md)).

Naming: the generated controller/route name is derived from the **`TGetFullDto`** type name with `Get`, `Full`, `List`, `Dto` and `V<n>` suffixes removed, so `BlogGetFullDto` -> `/Blog`, `PostGetFullDto` -> `/Post`, `AuthorDtoV1` -> `/Author`. Name your DTOs accordingly.

## Step 12: Verify
1. Build: `dotnet build` (fix Mapperly/EF diagnostics first).
2. Run the API (`dotnet run --project src\MyApi`, or the Aspire AppHost — see the `aspire` skill in `.agents\skills`). With `MigrateDatabaseDuringStartup` the migration is applied on start; health: `GET /health/startup`, `/health/ready`, `/health/live`.
3. Call it:
   - Root `GET /` lists links to all resources (HAL). Your resource must appear there with its curie-prefixed relation (default curie is configured by `RESTworld:Curie`).
   - Open `https://hal-explorer.com/#uri=https://localhost:<port>` (ExampleBlog: `https://localhost:5432`) and follow the link to the new resource, or open `/swagger`.
   - Check: list (`GET /<Name>`, OData `$filter`, `$orderby`, `$top`), single (`GET /<Name>/{id}`), create (POST with the HAL-Forms template), update (PUT with the current `timestamp`; a stale value must return 409), delete.
   - Request a specific version with `Accept: application/hal+json; v=2` (or `?v=2` when `RESTworld:Versioning:AllowQueryParameterVersioning` is true).
4. With `RESTworld:DisableAuthorization: true` (as in ExampleBlog's `appsettings.json`) authorization handlers are skipped; test them with it set to `false`.

## Common Mistakes
- Forgetting `AddCrudMapper`/`AddReadMapper` -> DI error on first request.
- Mapper generic arguments differ from the pipeline's -> DI cannot resolve the mapper.
- Entity not deriving from `ConcurrentEntityBase` -> pipeline generic constraint error.
- No migration -> `/health/startup` stays unhealthy and requests fail with invalid object name.
- Migration made but DTO shape changed for existing clients -> add a version instead ([restworld-add-api-version](../restworld-add-api-version/SKILL.md)).

## Reference
- [Getting Started](https://github.com/wertzui/RESTworld/blob/main/doc/getting-started.md)
- [Pipeline Overview](https://github.com/wertzui/RESTworld/blob/main/doc/pipeline-overview.md)
- [Choosing a Pipeline](https://github.com/wertzui/RESTworld/blob/main/doc/choosing_a_pipeline.md)
- [Entities and Database](https://github.com/wertzui/RESTworld/blob/main/doc/entities-and-database.md)
- [DTO Types and Roles](https://github.com/wertzui/RESTworld/blob/main/doc/dto-types.md)
- [Mapping and Versioning](https://github.com/wertzui/RESTworld/blob/main/doc/mapping-and-versioning.md)
- [How-To Guides](https://github.com/wertzui/RESTworld/blob/main/doc/how-to.md)
