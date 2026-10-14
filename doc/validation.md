# Validation

RESTworld runs your validators when a resource is created or updated through a CRUD pipeline. Use them for rules that DataAnnotations on the DTO cannot express, for example rules that need the database or compare the incoming DTO with the stored entity. Failed validations are returned to the client as an HTTP `400` with field-level error messages.

Validators run inside `CrudServiceBase` (see [Custom Services](services.md)). Read-only pipelines have no validation.

## Writing a validator

Derive from one of the base classes in `RESTworld.Business.Validation.Abstractions`. All methods are virtual and succeed by default, so you only override what you need.

| Rule applies to | Base class | Interface |
|---|---|---|
| Create and update | `ValidatorBase<TCreateDto, TUpdateDto, TEntity>` | `IValidator<TCreateDto, TUpdateDto, TEntity>` |
| Create only | `CreateValidatorBase<TCreateDto, TEntity>` | `ICreateValidator<TCreateDto, TEntity>` |
| Update only | `UpdateValidatorBase<TUpdateDto, TEntity>` | `IUpdateValidator<TUpdateDto, TEntity>` |

| Method | Runs |
|---|---|
| `ValidateBeforeCreateAsync(TCreateDto dto, CancellationToken)` | Before the DTO is mapped to an entity |
| `ValidateAfterCreateAsync(TCreateDto dto, TEntity entity, CancellationToken)` | After mapping and adding the entity to the context, before saving |
| `ValidateBeforeUpdateAsync(TUpdateDto dto, TEntity entity, CancellationToken)` | After the entity was loaded from the database, before the DTO is applied |
| `ValidateAfterUpdateAsync(TUpdateDto dto, TEntity entity, CancellationToken)` | After the DTO was applied to the entity, before saving |
| `ValidateCollectionBeforeCreateAsync(IEnumerable<TCreateDto>, CancellationToken)` | Before a bulk create |
| `ValidateCollectionAfterCreateAsync(IEnumerable<(TCreateDto dto, TEntity entity)>, CancellationToken)` | After mapping a bulk create |
| `ValidateCollectionBeforeUpdateAsync(IEnumerable<(TUpdateDto dto, TEntity entity)>, CancellationToken)` | Before applying a bulk update |
| `ValidateCollectionAfterUpdateAsync(IEnumerable<(TUpdateDto dto, TEntity entity)>, CancellationToken)` | After applying a bulk update |

The single-item methods are not called for bulk requests (an array in the request body, or a JSON Patch that creates or updates several entries). If bulk requests must be validated, override the collection methods as well. When a "before" validation fails, the "after" validation and saving are skipped.

Every method returns `Task<IValidationResults>`.

## Returning results

`ValidationResults` (`RESTworld.Business.Validation`) collects failures per path. The path is the DTO property name; use an empty string for an error that concerns the whole object.

```csharp
ValidationResults.Validate(condition, "PropertyName", "Message")      // IValidationResults, failure if condition is false
    .Validate(otherCondition, "OtherProperty", "Message")             // extension method, chainable
    .Validate(thirdCondition, new[] { "A", "B" }, "Message");         // one message for several paths
```

| API | Description |
|---|---|
| `ValidationResults.Validate(bool validationSucceeded, string path, string message)` | Static entry point. Also an overload with `IEnumerable<string> paths`. |
| `ValidationResults.ValidateAsync(Task<bool>, string path, string message)` | Awaits the condition first. |
| `new ValidationResults()`, `new ValidationResults(path, message)`, `new ValidationResults(path, exception)`, `new ValidationResults(IValidationResults)` | Create and fill manually. |
| `AddValidationFailure(path, message)` / `AddValidationFailure(path, exception)` | Add a failure. |
| `AddValidationFailures(IValidationResults)` | Merge another result. |
| `AddCollectionValidationFailures(IEnumerable<IValidationResults>)` | Merge results per collection item; paths are prefixed with `[index].`. |
| `ValidationSucceeded` | `true` when there are no failures. |
| `SuccessfullValidationResults.Instance` | Shared, read-only "everything is fine" result. Inside the base classes use the protected `CompletedSuccessfulValidation`. |

## Example

`PostValidatior` from the ExampleBlog (`ExampleBlog.Business/Validation/PostValidationHandler.cs`):

```csharp
public class PostValidatior : ValidatorBase<PostCreateDto, PostUpdateDto, Post>
{
    private readonly IDbContextFactory<BlogDatabase> _contextFactory;

    public PostValidatior(IDbContextFactory<BlogDatabase> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override async Task<IValidationResults> ValidateBeforeCreateAsync(PostCreateDto dto, CancellationToken cancellationToken)
    {
        var postCount = await _contextFactory.Parallel().Set<Post>().CountAsync(p => p.AuthorId == dto.AuthorId, cancellationToken);

        return ValidationResults
            .Validate(postCount < 42, nameof(PostCreateDto.AuthorId), "There must be no more than 42 posts from the same author for whatever reason.")
            .Validate(!string.Equals(dto.Headline, dto.Text, System.StringComparison.OrdinalIgnoreCase), "", "Headline and Text must be different.");
    }

    public override Task<IValidationResults> ValidateBeforeUpdateAsync(PostUpdateDto dto, Post entity, CancellationToken cancellationToken)
    {
        return Task.FromResult(ValidationResults.Validate(dto.AuthorId == entity.AuthorId, nameof(PostUpdateDto.AuthorId), "The author of a post cannot be changed."));
    }
}
```

Validators are resolved from dependency injection and can inject anything registered there. All validators registered for the same DTOs run in parallel and their failures are merged, so they must not share state; create a context per validator (`CreateDbContext()` or `_contextFactory.Parallel()`). Return failures instead of throwing: an exception inside a validator is wrapped into a `CouldNotExecuteValidationException` and results in a `500`.

## Registration

`builder.AddRestWorld()` already registers the validation infrastructure through `AddValidationAndErrorHandling()` (the open generic `IValidationService<,,>`, the `IErrorResultFactory` and the problem details factory). You only register your validators on the `IHostApplicationBuilder`:

```csharp
rwBuilder.AddValidator<PostValidatior, PostCreateDto, PostUpdateDto, Post>();
rwBuilder.AddCreateValidator<MyCreateValidator, PostCreateDto, Post>();
rwBuilder.AddUpdateValidator<MyUpdateValidator, PostUpdateDto, Post>();
```

`AddValidator` registers the class for create and update. You can register several validators for the same DTOs. The type arguments must match the ones of the pipeline. The `IServiceCollection` variants of these methods are marked obsolete.

The default `CrudServiceBase` receives the `IValidationService<TCreateDto, TUpdateDto, TEntity>` automatically. If you write your own service, take it as a constructor parameter and pass it to the base constructor, as `PostService` does.

## What the client receives

A failed validation produces an HTTP `400` response. The body is a HAL resource whose state is a `ValidationProblemDetails`: `status`, `title`, `type`, `traceId`, a `_links.self` link and `errors`, which maps every path to an array of messages. Property names follow the configured JSON naming policy (camelCase by default); an empty path stays an empty key.

```json
{
  "status": 400,
  "title": "BadRequest",
  "errors": {
    "authorId": ["There must be no more than 42 posts from the same author for whatever reason."],
    "": ["Headline and Text must be different."]
  }
}
```

Other failures use the same structure:

- A concurrency conflict (`DbUpdateConcurrencyException`, for example a stale `Timestamp`) is returned as `409` with a validation error on the concurrency property.
- A foreign key violation is returned as a failed response for the DTO property that holds the foreign key.
- Failures that are not validation errors (for example `404`) are returned as problem details with `status`, `title` and `detail`.

The Angular client maps the error paths to the fields of the HAL-Forms form.

## Related

- [Custom Services](services.md) – `ServiceResponse.FromFailedValidation` and the service override points
- [DTO Types and Roles](dto-types.md) – which DTO each validator method receives
- [Custom Controllers](custom-controllers.md) – `IErrorResultFactory` for your own error responses
- [Getting Started](getting-started.md) – registering validators in `Program.cs`
