---
name: restworld-halforms-customization
description: 'Customize the HAL-FORMS templates that a RESTworld API generates. Use when asked to add an extra form/template to a resource, embed related data as a form, change what a HAL-Forms response contains, implement IFormsResourceGenerationCustomization, call AddFormsResourceGenerationCustomization, fix OData enum values in the Search form (ODataEnumFix), customize a single form property (placeholder, options, read-only) with PropertyTemplateGenerationCustomization, or register AddForeignKeyForFormTo for foreign-key dropdown options. Also handles: understanding when customizations run (list Search form vs. single-resource form), why a customization is registered as singleton, and how foreign-key navigation properties on DTOs produce options links.'
argument-hint: 'Describe which form (single resource or list Search/Edit) should change and how. Optionally name the DTO type and the target RESTworld project (the one with Program.cs).'
---

# RESTworld: Customize Generated HAL-FORMS

RESTworld generates a HAL-FORMS document for a resource if the client sends an `Accept` header that prefers HAL-Forms over plain HAL. You can hook into that generation to add templates, change properties, or post-process the result.

## When to Use
- A form needs an additional template (e.g. an embedded read-only "Author" form on a post)
- The generated `Search` form of a list endpoint needs post-processing
- A single form property needs a placeholder, different options, or to be excluded
- You need to register a foreign-key options factory for a type that is not the `TGetListDto` of a pipeline

## Key Rules

1. **Interface**: implement `IFormsResourceGenerationCustomization` (`HAL.AspNetCore.Forms.Customization`). Members: `bool Exclusive`, `int Order`, `bool AppliesTo<TValue, TTemplate>(FormsResource formsResource, TValue value, HttpMethod method, string title, string contentType, string action, string? controller, object? routeValues)` and `ValueTask ApplyAsync<TValue, TTemplate>(<same parameters>, IFormFactory formFactory)`.
2. **Registration**: `rwBuilder.AddFormsResourceGenerationCustomization<TCustomization>()` in `Program.cs` (extension on `IHostApplicationBuilder`, namespace `Microsoft.Extensions.DependencyInjection`). It is registered as a **singleton** — constructor dependencies must be singleton-safe (`ILinkFactory` is).
3. **Execution**: for each form, the registered customizations are iterated in registration order; each one whose `AppliesTo(...)` returns `true` gets `ApplyAsync(...)`. HAL registers its default customization first (it adds the `default` template when the resource has no templates yet). With the HAL version RESTworld uses (HAL.AspNetCore.OData 22.0.0) `Order` and `Exclusive` are **not evaluated** for resource customizations — return `false` / any number and do not rely on them for sequencing.
4. **When it runs**: only if the request accepts HAL-Forms. Single resources: `TValue` is the DTO type, `title` is `"View"`. List endpoints: `TValue` is `Page?`, `TTemplate` is the list DTO type, `title` is `"Search"`, `contentType` is `application/x-www-form-urlencoded`, and `formsResource.Embedded["items"]` is filled.
5. **Keep `AppliesTo` cheap and precise** (type check on `value`, or on title/content type) — it is evaluated for every form.
6. **Add templates** with `formsResource.Templates["Name"] = template`; create them with `formFactory.CreateFormAsync<TValue, TTemplate>(value, target, HttpMethod, title)`.

## Steps

### 1. Add an extra template (single resource)
From `ExampleBlog/HalFormsCustomizations/AuthorForPostCustomization.cs`:
```csharp
public class AuthorForPostCustomization : IFormsResourceGenerationCustomization
{
    private readonly ILinkFactory _linkFactory;
    public AuthorForPostCustomization(ILinkFactory linkFactory) => _linkFactory = linkFactory;

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
Usings: `HAL.AspNetCore.Abstractions`, `HAL.AspNetCore.Forms.Abstractions`, `HAL.AspNetCore.Forms.Customization`, `HAL.AspNetCore.Utils`, `HAL.Common.Forms`, `RESTworld.AspNetCore.Controller`, `System.Net.Http`.

### 2. Register it
```csharp
var rwBuilder = builder.AddRestWorld();
rwBuilder.AddFormsResourceGenerationCustomization<AuthorForPostCustomization>();
```
**Versioned APIs**: the customization matches on the DTO type, so the V1 DTO needs its own class (`AuthorForPostCustomizationV1` matches `PostWithAuthorDtoV1`) and its own registration — exactly as `ExampleBlog/Program.cs` does.

### 3. Post-process the list `Search` form (ODataEnumFix)
`ExampleBlog/HalFormsCustomizations/ODataEnumFix.cs` shows the pattern: `AppliesTo` returns `title == "Search" && contentType == "application/x-www-form-urlencoded" && formsResource.Embedded is not null`; `ApplyAsync` loops `formsResource.Templates`, picks the `"Search"` template and rewrites each enum property's `Options` so values read `'Value'` (OData needs PascalCase names in single quotes). Use it as a template for your own Search-form rewrites. Note: `ODataEnumFix` exists in ExampleBlog but is **not registered** in its `Program.cs`, because HAL's `ODataFormFactory` (22.0.0) already applies a built-in enum fix to the Search form. Only register your own variant if you need additional changes.

### 4. Customize a single property
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
// Program.cs
rwBuilder.AddPropertyTemplateGenerationCustomization<HeadlinePlaceholder>();
```
Base constructor: `(Expression<Func<TDto, TProperty>> property, int order = 1, bool exclusive = false)`. Property customizations *do* honour `Order`; only one `exclusive` customization may apply to a property (otherwise an `InvalidOperationException` is thrown). Override `IncludeAsync(...)` to return `false` to drop the property from the template. The value counterpart is `AddPropertyValueGenerationCustomization<T>()`.

### 5. Foreign-key options
`AddCrudPipeline…` and `AddReadPipeline…` already call `AddForeignKeyForFormTo<TGetListDto>()`. Call it yourself only for DTOs without a pipeline:
```csharp
rwBuilder.AddForeignKeyForFormTo<BlogGetListDto>();
```
It registers a `CrudForeignKeyLinkFactory<TListDto>` that links to the `GetList` action of the controller named after the DTO (`RestControllerNameConventionAttribute.CreateNameFromType`). How a navigation property pairs with its id property (`AuthorId` → `Author`, `[ForeignKey]`, nullability, requiredness) is documented in [dto-types.md](https://github.com/wertzui/RESTworld/blob/main/doc/dto-types.md#foreign-keys-and-hal-forms) — see there instead of duplicating rules.

## Verification Checklist
- [ ] Customization class implements all four members
- [ ] Constructor dependencies are singleton-safe
- [ ] `AppliesTo` is a narrow type/title check
- [ ] Registered with `AddFormsResourceGenerationCustomization<T>()` — one class (and one registration) per DTO version
- [ ] Requested with `Accept: application/prs.hal-forms+json` and the new template appears under `_templates`
- [ ] Remember: with plain `application/hal+json` customizations do not run

## Related
- [Forms customization guide](https://github.com/wertzui/RESTworld/blob/main/doc/forms-customization.md)
- [DTO types and FK options](https://github.com/wertzui/RESTworld/blob/main/doc/dto-types.md) · skill `restworld-create-dtos`
- Custom controllers that return forms: skill `restworld-custom-controller`
