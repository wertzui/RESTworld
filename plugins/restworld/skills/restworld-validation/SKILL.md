---
name: restworld-validation
description: 'Add custom validation to a RESTworld CRUD endpoint. Use when asked to validate DTOs before or after create/update, reject a request with a 400 validation error, add business-rule checks (uniqueness, limits, immutable fields, cross-field rules), write a validator class, derive from ValidatorBase, CreateValidatorBase or UpdateValidatorBase, return ValidationResults / IValidationResults, register a validator with AddValidator, AddCreateValidator or AddUpdateValidator, understand AddValidationAndErrorHandling, ValidationService or the problem details / errors dictionary returned to the client, or validate collections (ValidateCollectionBeforeCreateAsync etc.).'
argument-hint: 'Describe the rule(s) to enforce (e.g. "a post headline must differ from its text", "an author cannot change"). Provide entity and Create/Update DTO type names, and whether the rule applies to create, update, or both.'
---

# RESTworld: Validation

## When to Use
- A rule goes beyond what DataAnnotations on the DTO can express (needs the database, the existing entity, or compares DTO and entity).
- You must return field-level errors to the client with HTTP 400.

Validators run inside `CrudServiceBase` (create and update, single and collection). They do **not** run for read-only pipelines or custom services unless you call `IValidationService` yourself.

## Pick the base class (namespace `RESTworld.Business.Validation.Abstractions`)

| Rule applies to | Derive from | Interface |
|---|---|---|
| Create and update | `ValidatorBase<TCreateDto, TUpdateDto, TEntity>` | `IValidator<TCreateDto, TUpdateDto, TEntity>` |
| Create only | `CreateValidatorBase<TCreateDto, TEntity>` | `ICreateValidator<TCreateDto, TEntity>` |
| Update only | `UpdateValidatorBase<TUpdateDto, TEntity>` | `IUpdateValidator<TUpdateDto, TEntity>` |

All methods are `virtual` and return `Task<IValidationResults>` (default: success). Override only what you need.

| Method | When it runs |
|---|---|
| `ValidateBeforeCreateAsync(TCreateDto dto, ct)` | Before the entity is mapped/added |
| `ValidateAfterCreateAsync(TCreateDto dto, TEntity entity, ct)` | After mapping and adding to the context, before saving |
| `ValidateBeforeUpdateAsync(TUpdateDto dto, TEntity entity, ct)` | Entity loaded from the database, before the DTO is mapped onto it |
| `ValidateAfterUpdateAsync(TUpdateDto dto, TEntity entity, ct)` | After mapping onto the entity, before saving |
| `ValidateCollectionBeforeCreateAsync(IEnumerable<TCreateDto>, ct)` / `ValidateCollectionAfterCreateAsync(IEnumerable<(TCreateDto dto, TEntity entity)>, ct)` | Same for POST with an array |
| `ValidateCollectionBeforeUpdateAsync` / `ValidateCollectionAfterUpdateAsync(IEnumerable<(TUpdateDto dto, TEntity entity)>, ct)` | Same for PUT with an array |

If a "before" validation fails, the "after" validation and saving are skipped. Single-item methods are not called for collection requests: **override the collection methods too** if collection requests must be validated.

## Building results
`RESTworld.Business.Validation.ValidationResults`:
- `ValidationResults.Validate(bool validationSucceeded, string path, string message)` → `IValidationResults`; chain with the extension `.Validate(bool, string path, string message)` or `.Validate(bool, IEnumerable<string> paths, string message)`.
- `ValidationResults.ValidateAsync(Task<bool>, path, message)`.
- `new ValidationResults(path, message)`, `AddValidationFailure(path, message)` (also `(path, Exception)`), `AddValidationFailures(IValidationResults)`, `AddCollectionValidationFailures(IEnumerable<IValidationResults>)` (prefixes paths with `[index].`).
- `SuccessfullValidationResults.Instance` is the shared "ok" result (read-only). The base classes expose it as the protected `CompletedSuccessfulValidation` task.
- `path` is the DTO property name (`nameof(PostCreateDto.AuthorId)`); use `""` for an error on the whole object.

## Example (ExampleBlog `PostValidatior`, file `Validation/PostValidationHandler.cs`)
```csharp
public class PostValidatior : ValidatorBase<PostCreateDto, PostUpdateDto, Post>
{
    private readonly IDbContextFactory<BlogDatabase> _contextFactory;
    public PostValidatior(IDbContextFactory<BlogDatabase> contextFactory) => _contextFactory = contextFactory;

    public override async Task<IValidationResults> ValidateBeforeCreateAsync(PostCreateDto dto, CancellationToken cancellationToken)
    {
        var postCount = await _contextFactory.Parallel().Set<Post>().CountAsync(p => p.AuthorId == dto.AuthorId, cancellationToken);

        return ValidationResults
            .Validate(postCount < 42, nameof(PostCreateDto.AuthorId), "There must be no more than 42 posts from the same author for whatever reason.")
            .Validate(!string.Equals(dto.Headline, dto.Text, StringComparison.OrdinalIgnoreCase), "", "Headline and Text must be different.");
    }

    public override Task<IValidationResults> ValidateBeforeUpdateAsync(PostUpdateDto dto, Post entity, CancellationToken cancellationToken)
        => Task.FromResult(ValidationResults.Validate(dto.AuthorId == entity.AuthorId, nameof(PostUpdateDto.AuthorId), "The author of a post cannot be changed."));
}
```
Validators are scoped services and may inject `IDbContextFactory<TContext>`, `IUserAccessor`, etc. All registered validators for the same DTOs run **in parallel** (`Task.WhenAll`) and their failures are merged, so they must not share state (use `_contextFactory.Parallel()` or `CreateDbContext()`). If a validator throws, the request fails with a 500 (`CouldNotExecuteValidationException`); return failures instead of throwing.

## Registration
`builder.AddRestWorld()` already calls `AddValidationAndErrorHandling()` (registers `IValidationService<,,>` as open generic, `IErrorResultFactory`, and the problem details factory). You only register validators (`using Microsoft.Extensions.DependencyInjection;`):

```csharp
rwBuilder.AddValidator<PostValidatior, PostCreateDto, PostUpdateDto, Post>();      // IValidator → create + update
rwBuilder.AddCreateValidator<MyCreateValidator, PostCreateDto, Post>();
rwBuilder.AddUpdateValidator<MyUpdateValidator, PostUpdateDto, Post>();
```
`AddValidator` registers the class as `ICreateValidator`, `IUpdateValidator` and `IValidator`. Multiple validators per DTO are allowed. The `IServiceCollection` variants of these methods are `[Obsolete]`; use the builder.
The service must be a `CrudServiceBase`-derived class that passes the injected `IValidationService<TCreateDto, TUpdateDto, TEntity>` to `base` (the default `CrudPipeline` does; custom services in the example do too).

## What the client receives
Failed validation → `ServiceResponse.FromFailedValidation` → **HTTP 400** with a HAL `Resource` whose state is a `ValidationProblemDetails`:
- `status`, `title` (the status name, e.g. `BadRequest`), `type`, `traceId`, a `_links.self` link, and `errors`: an object mapping each path to an array of messages.
- Property names in `errors` follow the configured JSON naming policy (camelCase by default: `authorId`); the `""` path stays an empty key.
- A concurrency conflict (`DbUpdateConcurrencyException`) is reported as **409** with the same structure; a foreign key violation is mapped to the offending DTO property.

```json
{ "status": 400, "title": "BadRequest",
  "errors": { "authorId": ["There must be no more than 42 posts from the same author for whatever reason."],
              "": ["Headline and Text must be different."] } }
```
The HAL Forms client (Angular) shows these messages next to the matching form fields.
To produce your own errors elsewhere (custom controller), inject `IErrorResultFactory` and call `CreateError(...)` → see [restworld-custom-controller](../restworld-custom-controller/SKILL.md).

## Verification checklist
- [ ] Validator derives from the right base; overridden signatures match (including the collection variants if needed).
- [ ] Failures returned via `ValidationResults`, `path` equals the DTO property name.
- [ ] Registered with `AddValidator` / `AddCreateValidator` / `AddUpdateValidator` for the exact DTO and entity types of the pipeline.
- [ ] The service passes `IValidationService` to `CrudServiceBase`.
- [ ] POST/PUT with an invalid payload returns 400 and the expected `errors` keys.

## Reference
- [Validation](https://github.com/wertzui/RESTworld/blob/main/doc/validation.md) · [Services](https://github.com/wertzui/RESTworld/blob/main/doc/services.md) · [DTO Types](https://github.com/wertzui/RESTworld/blob/main/doc/dto-types.md) · [Getting Started](https://github.com/wertzui/RESTworld/blob/main/doc/getting-started.md)
- Related skills: [restworld-custom-service](../restworld-custom-service/SKILL.md), [restworld-create-dtos](../restworld-create-dtos/SKILL.md)
