# Customizing HAL-FORMS

RESTworld answers requests that accept HAL-Forms (`Accept: application/prs.hal-forms+json`) with a generated form document: one or more templates describing the fields, the HTTP method and the target. The Angular client renders these templates; any other HAL-Forms client can use them too. You can extend the generation to add templates, change single fields or post-process the result.

For the plain, non-customized behavior (labels, required fields, foreign-key dropdowns) read [DTO Types and Roles](dto-types.md) first — most forms need no customization at all, only the right DTO annotations.

## Customize the whole form: `IFormsResourceGenerationCustomization`

Implement `IFormsResourceGenerationCustomization` (namespace `HAL.AspNetCore.Forms.Customization`):

| Member | Purpose |
|---|---|
| `bool AppliesTo<TValue, TTemplate>(FormsResource formsResource, TValue value, HttpMethod method, string title, string contentType, string action, string? controller, object? routeValues)` | Decides whether `ApplyAsync` runs for the form currently being generated. |
| `ValueTask ApplyAsync<TValue, TTemplate>(…same parameters…, IFormFactory formFactory)` | Modifies `formsResource` (add or change templates). Use `formFactory.CreateFormAsync<TValue, TTemplate>(value, target, method, title)` to build templates. |
| `bool Exclusive`, `int Order` | Part of the interface. With the HAL version used by RESTworld (HAL.AspNetCore.OData 22.0.0) they are not evaluated for resource customizations: customizations run in registration order. Return `false` and a number such as `1`. |

What you receive in `AppliesTo`/`ApplyAsync`:

| Form | `TValue` | `title` | `contentType` |
|---|---|---|---|
| Single resource (GET one, after POST/PUT) | the DTO type | `"View"` | `application/json` |
| List endpoint (search + edit forms) | `Page?` (`TTemplate` is the list DTO) | `"Search"` | `application/x-www-form-urlencoded` (`formsResource.Embedded["items"]` holds the items) |

Customizations only run when HAL-Forms is requested. Plain `application/hal+json` responses are never touched.

### Example: embed the author as a read-only form

From `src/Example/ExampleBlog/ExampleBlog/HalFormsCustomizations/AuthorForPostCustomization.cs`:

```csharp
public class AuthorForPostCustomization : IFormsResourceGenerationCustomization
{
    private readonly ILinkFactory _linkFactory;

    public AuthorForPostCustomization(ILinkFactory linkFactory)
    {
        _linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
    }

    public bool Exclusive => false;
    public int Order => 1;

    public bool AppliesTo<TValue, TTemplate>(FormsResource formsResource, TValue value, HttpMethod method, string title, string contentType, string action, string? controller, object? routeValues)
        => value is PostWithAuthorDto;

    public async ValueTask ApplyAsync<TValue, TTemplate>(FormsResource formsResource, TValue value, HttpMethod method, string title, string contentType, string action, string? controller, object? routeValues, IFormFactory formFactory)
    {
        if (value is PostWithAuthorDto dto)
        {
            var authorLink = _linkFactory.Create(
                ActionHelper.StripAsyncSuffix(nameof(ReadController<,,,>.GetAsync)),
                RestControllerNameConventionAttribute.CreateNameFromType<AuthorDto>(),
                new { id = dto.AuthorId }).Href ?? "";
            var authorForm = await formFactory.CreateFormAsync<AuthorDto?, TTemplate>(dto.Author, authorLink, HttpMethod.Get, "Author");
            formsResource.Templates["The author"] = authorForm;
        }
    }
}
```

Register it in `Program.cs`:

```csharp
var rwBuilder = builder.AddRestWorld();
rwBuilder.AddFormsResourceGenerationCustomization<AuthorForPostCustomization>();
```

Things to know:

- The customization is registered as a **singleton**; do not inject scoped services.
- `AppliesTo` runs for every generated form. Keep it a cheap type or title check.
- A customization matches on the DTO type. With [API versions](mapping-and-versioning.md) that use different DTOs, write one class and one registration per DTO (`AuthorForPostCustomizationV1` for `PostWithAuthorDtoV1`).

### Example: post-process the list `Search` form

`ODataEnumFix` (same folder) shows how to target the list search form: `AppliesTo` checks `title == "Search" && contentType == "application/x-www-form-urlencoded" && formsResource.Embedded is not null`, `ApplyAsync` then rewrites the `Options` of enum properties in the `"Search"` template so values read `'Published'` as OData requires. `ODataEnumFix` is not registered in the example `Program.cs`, because HAL's `ODataFormFactory` already applies an equivalent fix; use the class as a pattern for other Search-form rewrites.

## Customize a single field

Derive from `PropertyTemplateGenerationCustomization<TDto, TProperty>` and register it:

```csharp
public class HeadlinePlaceholder : PropertyTemplateGenerationCustomization<PostCreateDto, string>
{
    public HeadlinePlaceholder() : base(d => d.Headline) { }

    public override ValueTask ApplyAsync(PropertyInfo propertyInfo, Property halFormsProperty, IFormTemplateFactory formTemplateFactory)
    {
        halFormsProperty.Placeholder = "A catchy headline";
        return ValueTask.CompletedTask;
    }
}

rwBuilder.AddPropertyTemplateGenerationCustomization<HeadlinePlaceholder>();
```

- The base constructor is `(Expression<Func<TDto, TProperty>> property, int order = 1, bool exclusive = false)`. Property customizations honour `order`; the default generation runs at `0`.
- Set `exclusive: true` to replace the default generation for that property. Only one exclusive customization may apply to a property, otherwise an `InvalidOperationException` is thrown.
- Override `IncludeAsync(...)` and return `false` to remove the field from the template.
- The counterpart for the *values* of fields is `AddPropertyValueGenerationCustomization<T>()`.

## Foreign-key options

For fields that reference another resource, the form contains an `options` element with a link to that resource's list endpoint. The pairing rules (`AuthorId` → `Author`, `[ForeignKey]`, nullable navigation properties, required vs. optional) are described in [DTO Types and Roles](dto-types.md#foreign-keys-and-hal-forms).

The link comes from `AddForeignKeyForFormTo<TListDto>()`, which every `AddCrudPipeline…` and `AddReadPipeline…` call already invokes for its `TGetListDto`. It registers a `CrudForeignKeyLinkFactory<TListDto>` that points to the `GetList` action of the controller named after the DTO. Only call it yourself for list DTOs without a pipeline:

```csharp
rwBuilder.AddForeignKeyForFormTo<BlogGetListDto>();
```

## Checklist

- Implemented all four members of `IFormsResourceGenerationCustomization`
- `AppliesTo` is narrow; constructor dependencies are singleton-safe
- Registered with `AddFormsResourceGenerationCustomization<T>()` (one per DTO version)
- Tested with `Accept: application/prs.hal-forms+json`; the template shows under `_templates`

Related: [Pipeline Overview](pipeline-overview.md), [Mapping and Versioning](mapping-and-versioning.md), [Angular Client Development](client-development-angular.md).
