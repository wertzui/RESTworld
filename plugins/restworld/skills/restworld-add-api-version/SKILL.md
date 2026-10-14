---
name: restworld-add-api-version
description: 'Introduce a new API version (V2, V3, ...) of an existing RESTworld endpoint without breaking existing clients. Use when asked to add API versioning, create a v2 of an endpoint, change a DTO in a breaking way (rename or split a property, remove a field), deprecate an old version, keep V1 working, register a pipeline with ApiVersion, use [ApiVersion] on custom controllers, version custom services, authorization handlers or HAL-Forms customizations, set the default version, or understand how clients request a version via the Accept header (application/hal+json; v=2) or query parameter. Uses AuthorDtoV1 / AuthorDto from ExampleBlog as the worked example.'
argument-hint: 'Name the endpoint/entity, the target version (e.g. 2) and what changes between the current and the new version (DTO property changes, behavior changes). Mention whether the old version should be marked deprecated.'
---

# RESTworld: Add a New API Version

## When to Use
- A change to a DTO or behavior would break existing clients
- You are asked to "add v2", "deprecate v1" or "keep the old contract working"
- You need to align a versioned custom controller/service/authorization handler/form customization with the pipeline versions

Non-breaking changes (new optional property, new endpoint) do **not** need a new version.

## Rules
1. Follow **Rule B** of [restworld-choose-pipeline](../restworld-choose-pipeline/SKILL.md): never infer a version number; start from the N-1 implementation and apply only the described differences; V1 is registered with `new ApiVersion(1, 0)` (ExampleBlog also passes `true` as `isDeprecated` once V2 exists), later versions with `new ApiVersion(N, 0)`.
2. **Every version gets its own DTO set and its own mapper registration.** Keep version logic in the mapper ([restworld-create-mapper](../restworld-create-mapper/SKILL.md)); avoid `if (version)` branches in services.
3. Both versions share **one entity and one table**. The database must be able to serve every version; migrate the schema to the newest shape and make the old DTOs derive their values from it.
4. DTO naming matters: the route is derived from the `TGetFullDto` type name with `Get`, `Full`, `List`, `Dto` and `V<n>` suffixes stripped, so `AuthorDtoV1` and `AuthorDto` both serve `/Author`. Keep this naming scheme for old versions (`...V1`).
5. Namespace for `ApiVersion` is `Asp.Versioning`.

## Steps (worked example: Author V1 -> V2)

### 1. Keep the old DTO, add the new one (`MyApi.Common/Dtos`)
```csharp
// V1: single Name   (AuthorDtoV1 : ChangeTrackingDtoBase)
public class AuthorDtoV1 : ChangeTrackingDtoBase
{
    public string? Email { get; set; }
    public string? Name { get; set; }
}

// V2: FirstName + LastName (AuthorDto : ChangeTrackingDtoBase)
[DisplayColumn(nameof(LastName))]
public class AuthorDto : ChangeTrackingDtoBase
{
    [Display(Name = "E-Mail")] public string? Email { get; set; }
    [Display(Name = "First name")] public string? FirstName { get; set; }
    [Display(Name = "Last name")] public string? LastName { get; set; }
}
```
Rename the current DTO to `...V1` **before** creating the new one (and update existing usages), then create the unsuffixed new DTO. See [restworld-create-dtos](../restworld-create-dtos/SKILL.md).

### 2. Change the entity and add a migration
Change the entity to the new shape (`Author` has `FirstName`, `LastName`, `Email`) and create the migration ([restworld-create-entity-and-dbcontext](../restworld-create-entity-and-dbcontext/SKILL.md)). ExampleBlog's `AuthorV2` migration renames `Name` to `LastName`, adds `FirstName` and updates the seed rows; edit generated migrations by hand so existing data is preserved (use `RenameColumn` and `UpdateData`/SQL instead of drop + add).

### 3. Add the mapper for the new version, adapt the old one
- New: mapper for `Author` <-> `AuthorDto` (identical property names, trivial).
- Old: the V1 mapper must compute `Name` from `FirstName`/`LastName` on read and split `Name` on write. ExampleBlog does this in `AutoMapperConfiguration` (`ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))` and the reverse split). This is legacy AutoMapper code that ExampleBlog keeps on purpose to show the migration path. New code should use a Mapperly mapper; keep the queryable projections translatable by EF Core.

### 4. Register both versions in `Program.cs`
```csharp
rwBuilder.AddCrudPipeline<BlogDatabase, Author, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1>(new ApiVersion(1, 0), true);
rwBuilder.AddCrudAutoMapper<Author, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1>();   // legacy; new code: AddCrudMapper<..., AuthorMapperV1>()
rwBuilder.AddCrudPipeline<BlogDatabase, Author, AuthorDto, AuthorDto, AuthorDto, AuthorDto, AuthorDto>(new ApiVersion(2, 0));
rwBuilder.AddCrudAutoMapper<Author, AuthorDto, AuthorDto, AuthorDto, AuthorDto, AuthorDto>();               // legacy; new code: AddCrudMapper<..., AuthorMapper>()
```
Signature: `AddCrudPipeline<...>(ApiVersion? apiVersion = null, bool isDeprecated = false)` (same for `AddReadPipeline`, and the `...WithAuthorization` variants). With `apiVersion` omitted the controller is API-version-neutral. `isDeprecated: true` calls `HasDeprecatedApiVersion`, so the version is listed in `api-deprecated-versions`. The `...WithCustomService...` pipeline methods have **no** version parameter; if a service-backed endpoint must be versioned, use a custom controller with `[ApiVersion]` (step 5).

### 5. Version custom controllers, services, authorization, form customizations
ExampleBlog's `postwithauthor` endpoint exists twice. Both controllers use the same route and differ by attribute:
```csharp
[Route("postwithauthor")]
[ApiVersion("2")]
public class MyCustomController : RestControllerBase { /* uses MyCustomService, PostWithAuthorDto */ }

[Route("postwithauthor")]
[ApiVersion("1", Deprecated = true)]
public class MyCustomController1 : RestControllerBase { /* uses MyCustomServiceV1, PostWithAuthorDtoV1 */ }
```
Per version also create (and register, `services.AddScoped<...>()`): the service (`MyCustomService` / `MyCustomServiceV1`), the authorization handler (`MyCustomAuthorizationHandler` / `MyCustomAuthorizationHandlerV1`, typed on the version's DTO), read mappers (`AddReadMapper` with Mapperly mappers for the version's DTOs; ExampleBlog still uses the legacy `AddReadAutoMapper`) and any `IFormsResourceGenerationCustomization` (`AuthorForPostCustomization` / `AuthorForPostCustomizationV1`, whose `AppliesTo` checks `value is PostWithAuthorDto` vs `PostWithAuthorDtoV1`, registered with `AddFormsResourceGenerationCustomization<T>()`). Details: [restworld-custom-controller](../restworld-custom-controller/SKILL.md), [restworld-custom-service](../restworld-custom-service/SKILL.md), [restworld-authorization-handler](../restworld-authorization-handler/SKILL.md), [restworld-halforms-customization](../restworld-halforms-customization/SKILL.md).

### 6. Configure defaults (`appsettings.json`)
```json
"RESTworld": { "Versioning": { "DefaultVersion": "2.0", "ParameterName": "v", "AllowQueryParameterVersioning": true } }
```
- `DefaultVersion` `"*"` (the default when unset) selects the latest version when the client sends none; a concrete value pins the default.
- `ParameterName` is the media-type parameter (default `v`); `AllowQueryParameterVersioning` additionally accepts `?v=1` for legacy clients.

### 7. Communicate to clients
- Clients select a version with `Accept: application/hal+json; v=1` (or `?v=1` if enabled). Responses carry `api-supported-versions` and `api-deprecated-versions` headers.
- .NET and Angular clients configure the version per API in `RESTworld:ClientSettings:ApiUrls[].Version` (ExampleBlog.Client uses `"Version": 2`). Point existing clients at the old version explicitly, upgrade them at their pace, and remove the old version (pipeline, DTOs, mappers, controllers, services) once no one uses it.

## Verification Checklist
- [ ] Old DTO renamed to `...V1`, new DTO has the unsuffixed name; both exist
- [ ] Migration preserves data and applies cleanly (`dotnet ef database update` or startup migration)
- [ ] One mapper registration per version, generic arguments match the pipeline
- [ ] `new ApiVersion(1, 0)` with `true` (deprecated) and `new ApiVersion(2, 0)` registered; custom controllers have `[ApiVersion(...)]`
- [ ] `GET /Author` with `Accept: application/hal+json; v=1` returns `name`; with `v=2` returns `firstName`/`lastName`
- [ ] `api-deprecated-versions` header lists 1; POST/PUT round-trips work in both versions
- [ ] Client settings updated where a client should move to the new version

## Reference
- [Mapping and Versioning](https://github.com/wertzui/RESTworld/blob/main/doc/mapping-and-versioning.md)
- [Configuration](https://github.com/wertzui/RESTworld/blob/main/doc/configuration.md)
- [Choosing a Pipeline](https://github.com/wertzui/RESTworld/blob/main/doc/choosing_a_pipeline.md)
- [.NET Client Development](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-dotnet.md) and [Angular Client Development](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-angular.md)
