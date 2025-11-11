using AutoFixture;
using AutoFixture.Kernel;
using HAL.AspNetCore;
using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.OData;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using RESTworld.AspNetCore.AutoFixture.Customizations;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Serialization;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace RESTworld.AspNetCore.Swagger;

/// <summary>
/// This filter adds examples for all <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>s and for the <see cref="HomeController"/> to the Open API description.
/// </summary>
/// <seealso cref="IOperationFilter" />
public class SwaggerExampleOperationFilter : IOperationFilter
{
    /// <summary>
    /// The fixture can be used to customize the examples created by this filter.
    /// </summary>
    public static Fixture Fixture { get; }
    private static readonly SpecimenContext _specimenContext;
    private static readonly ODataRawQueryOptions _oDataRawQueryOptions;
    private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;
    private readonly string _curieName;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;
    private readonly IODataQueryFactory _oDataQueryFactory;
    private readonly JsonSerializerOptions _serializerOptions;

    static SwaggerExampleOperationFilter()
    {
        Fixture = new();

        // AutoFixture cannot create DateOnly on its own
        Fixture.Customize<DateOnly>(composer => composer.FromFactory<DateTime>(DateOnly.FromDateTime));

        // Otherwise these will be really small
        Fixture.Customize<TimeOnly>(composer => composer.FromFactory<DateTime>(TimeOnly.FromDateTime));

        // Omit recursion
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior(3));

        Fixture
            .Customize(new DtoBaseCustomization())
            .Customize(new ChangeTrackingDtoBaseCustomization())
            .Customize(new HalFileCustomization())
            .Customize(new JsonIgnoreCustomization())
            .Customize(new IgnoreDataMemberCustomization());

        _specimenContext = new(Fixture);

        _oDataRawQueryOptions = new ODataRawQueryOptions();
        typeof(ODataRawQueryOptions).GetProperty(nameof(_oDataRawQueryOptions.Skip))!.SetValue(_oDataRawQueryOptions, "2");
        typeof(ODataRawQueryOptions).GetProperty(nameof(_oDataRawQueryOptions.Top))!.SetValue(_oDataRawQueryOptions, "3");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerExampleOperationFilter"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="linkGenerator">The link generator.</param>
    /// <param name="apiExplorer">The API explorer.</param>
    /// <param name="oDataQueryFactory">The o data query factory.</param>
    /// <param name="options">The options.</param>
    /// <exception cref="System.ArgumentNullException">
    /// options
    /// or
    /// httpContextAccessor
    /// or
    /// linkGenerator
    /// or
    /// apiExplorer
    /// or
    /// oDataQueryFactory
    /// </exception>
    public SwaggerExampleOperationFilter(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator, IApiDescriptionGroupCollectionProvider apiExplorer, IODataQueryFactory oDataQueryFactory, IOptions<RestWorldOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _curieName = options.Value.GetCurieOrDefault();

        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
        _apiExplorer = apiExplorer ?? throw new ArgumentNullException(nameof(apiExplorer));
        _oDataQueryFactory = oDataQueryFactory ?? throw new ArgumentNullException(nameof(oDataQueryFactory));

        _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        _serializerOptions.Converters.Add(new JsonStringEnumMemberConverter(JsonNamingPolicy.CamelCase));
    }

    /// <inheritdoc/>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Body Parameters
        AddExamplesForParameters(operation, context);

        AddExamplesForResponses(operation, context);
    }

    private static Type? GetBodyParameterType(MethodInfo method)
    {
        var bodyParameter = method.GetParameters().FirstOrDefault(p => p.GetCustomAttribute<FromBodyAttribute>(true) is not null);
        return bodyParameter?.ParameterType;
    }

    private void AddExampleForProblemResponse(int statusCodeInt, IOpenApiResponse response, OpenApiMediaType type)
    {
        type.Example = CreateExample(new ProblemDetails { Detail = response.Description, Title = response.Description, Status = statusCodeInt != 0 ? statusCodeInt : null });
    }

    private void AddExampleForSuccessfullHomeResponse(OpenApiMediaType type, ODataResourceFactory resourceFactory)
    {
        var resource = resourceFactory.CreateForHomeEndpointWithSwaggerUi(_curieName);
        type.Example = CreateExample(resource);
    }

    private void AddExampleForSuccessfullResponse(OperationFilterContext context, int statusCodeInt, OpenApiMediaType type)
    {
        var responseTypeFromApiDescription = context.ApiDescription.SupportedResponseTypes.FirstOrDefault(t => t.StatusCode == statusCodeInt);

        if (responseTypeFromApiDescription is not null)
        {
            var linkFactory = new LinkFactory(_linkGenerator, _httpContextAccessor, _apiExplorer);
            var resourceFactory = new ODataResourceFactory(linkFactory, _oDataQueryFactory);

            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                if (controllerActionDescriptor.ControllerTypeInfo.IsGenericType
                && controllerActionDescriptor.ControllerTypeInfo.GetGenericTypeDefinition() == typeof(ReadController<,,>))
                {
                    // Examples for READ Controller
                    AddExampleForSucessfullCrudResponse(type, linkFactory, resourceFactory, controllerActionDescriptor, RestControllerNameConventionAttribute.ReadControllerIndexOfListDtoType, RestControllerNameConventionAttribute.ReadControllerIndexOfFullDtoType);
                }
                else if (controllerActionDescriptor.ControllerTypeInfo.IsGenericType
                && controllerActionDescriptor.ControllerTypeInfo.GetGenericTypeDefinition() == typeof(CrudController<,,,,>))
                {
                    // Examples for CRUD Controller
                    AddExampleForSucessfullCrudResponse(type, linkFactory, resourceFactory, controllerActionDescriptor, RestControllerNameConventionAttribute.CrudControllerIndexOfListDtoType, RestControllerNameConventionAttribute.CrudControllerIndexOfFullDtoType);
                }
                else if (controllerActionDescriptor.ControllerTypeInfo == typeof(HomeController) && controllerActionDescriptor.ActionName == "Index")
                {
                    // Example for Home Controller
                    AddExampleForSuccessfullHomeResponse(type, resourceFactory);
                }
                else if (responseTypeFromApiDescription.Type is not null && responseTypeFromApiDescription.Type.IsAssignableTo(typeof(Resource)))
                {
                    // Example for any other, unknown controller
                    AddExampleForSuccessfullUnknownResourceResponse(type, responseTypeFromApiDescription.Type, resourceFactory, linkFactory, controllerActionDescriptor);
                }
            }

            if (type.Example is null && (type.Examples?.Count).GetValueOrDefault() == 0 && responseTypeFromApiDescription.Type is not null && responseTypeFromApiDescription.Type.IsAssignableTo(typeof(Resource)))
            {
                // Example for a response that does not come from a controller but is still a Resource
                AddExampleForSuccessfullUnknownResourceResponse(type, responseTypeFromApiDescription.Type, resourceFactory, linkFactory);
            }
        }
    }

    private void AddExampleForSuccessfullUnknownResourceResponse(OpenApiMediaType type, Type resourceType, ODataResourceFactory resourceFactory, ILinkFactory linkFactory, ControllerActionDescriptor? controllerActionDescriptor = null)
    {
        Resource resource;
        object? routeValues = null;
        if (resourceType.IsGenericType)
        {
            var stateType = resourceType.GenericTypeArguments[0];
            var state = Fixture.Create(stateType, _specimenContext);
            if (state is DtoBase dtoBase)
                routeValues = new { id = dtoBase.Id };

            resource = resourceFactory.Create(state);
        }
        else
        {
            resource = resourceFactory.Create();
        }

        try
        {
            linkFactory.AddSelfLinkTo(resource, controllerActionDescriptor?.ActionName ?? "Get", controllerActionDescriptor?.ControllerName, routeValues);
        }
        catch
        {
            // Creating the self link may fail if the route expects parameters that are not the Id of the resource.
        }

        type.Example = CreateExample(resource);
    }

    private void AddExampleForSucessfullCrudResponse(OpenApiMediaType type, LinkFactory linkFactory, ODataResourceFactory resourceFactory, ControllerActionDescriptor controllerActionDescriptor, int indexOfListDtoType, int indexOfFullDtoType)
    {
        var actionName = controllerActionDescriptor.ActionName;
        var controllerName = controllerActionDescriptor.ControllerName;
        var controllerType = controllerActionDescriptor.ControllerTypeInfo;

        if (actionName == "GetList")
        {
            var tListDto = controllerType.GenericTypeArguments[indexOfListDtoType];
            var embedded = Enumerable.Repeat<object?>(null, 3).Select(_ => Fixture.Create(tListDto, _specimenContext)).ToList();
            var resource = resourceFactory.CreateForODataListEndpointUsingSkipTopPaging(embedded, _ => Common.Constants.ListItems, e => ((DtoBase)e).Id, _oDataRawQueryOptions, 50, 10, controllerName);
            type.Example = CreateExample(resource);
        }
        else if (actionName is "Get" or "Post" or "Put" or "New")
        {
            var tFullDto = controllerType.GenericTypeArguments[indexOfFullDtoType];

            if (actionName is "Get" or "New")
            {
                // Get and New only return an object
                var state = Fixture.Create(tFullDto, _specimenContext);
                Resource resource;
                if (actionName == "New")
                {
                    if (state is DtoBase dtoBase)
                    {
                        // No Id and Timestamp when creating new resources
                        dtoBase.Id = default;
                        if (state is ConcurrentDtoBase concurrentDtoBase)
                        {
                            concurrentDtoBase.Timestamp = default;

                            // Change tracking values are set by the server, not by the client
                            if (dtoBase is ChangeTrackingDtoBase changeTrackingDtoBase)
                            {
                                changeTrackingDtoBase.CreatedAt = default;
                                changeTrackingDtoBase.CreatedBy = default;
                                changeTrackingDtoBase.LastChangedAt = default;
                                changeTrackingDtoBase.LastChangedBy = default;
                            }
                        }
                    }
                    resource = resourceFactory.CreateForEndpoint(state, action: "New", controller: controllerName);

                    resource.AddLink("save", linkFactory.Create("POST", action: "Post", controller: controllerName));
                }
                else
                {
                    resource = resourceFactory.CreateForEndpoint(state, controller: controllerName, routeValues: new { id = ((DtoBase)state).Id });
                }

                type.Example = CreateExample(resource);
            }
            else if (actionName is "Post" or "Put")
            {
                // Post and Put either return an object or a collection
                var states = Enumerable.Repeat<object?>(null, 3).Select(_ => Fixture.Create(tFullDto, _specimenContext)).Cast<ConcurrentDtoBase>().ToList();

                type.Examples ??= new Dictionary<string, IOpenApiExample>();
                type.Examples.Add("Single Object", new OpenApiExample { Value = CreateExample((Resource)resourceFactory.CreateForEndpoint(states[0], controller: controllerName, routeValues: new { id = states[0].Id })) });
                type.Examples.Add("Collection", new OpenApiExample { Value = CreateExample(resourceFactory.CreateForListEndpoint(states, _ => Common.Constants.ListItems, d => d.Id, controllerName)) });
            }
        }
    }

    private void AddExamplesForBodyParameter(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody?.Content is not null)
        {
            foreach (var type in operation.RequestBody.Content.Values)
            {
                if (type.Example is null)
                {
                    var bodyParameterType = GetBodyParameterType(context.MethodInfo);
                    if (bodyParameterType is null)
                        continue;

                    if (bodyParameterType.IsGenericType && bodyParameterType.GetGenericTypeDefinition() == typeof(SingleObjectOrCollection<>))
                    {
                        var stateType = bodyParameterType.GenericTypeArguments[0];
                        var states = Enumerable.Repeat<object?>(null, 3).Select(_ => Fixture.Create(stateType, _specimenContext)).ToList();
                        foreach (var state in states)
                        {
                            if (state is ConcurrentDtoBase dtoBase)
                            {
                                // No Id and Timestamp when creating new resources
                                if (context.ApiDescription.HttpMethod == "POST")
                                {
                                    dtoBase.Id = default;
                                    dtoBase.Timestamp = default;
                                }

                                // Change tracking values are set by the server, not by the client
                                if (dtoBase is ChangeTrackingDtoBase changeTrackingDtoBase)
                                {
                                    changeTrackingDtoBase.CreatedAt = default;
                                    changeTrackingDtoBase.CreatedBy = default;
                                    changeTrackingDtoBase.LastChangedAt = default;
                                    changeTrackingDtoBase.LastChangedBy = default;
                                }
                            }
                        }

                        type.Examples ??= new Dictionary<string, IOpenApiExample>();
                        type.Examples.Add("Single Object", new OpenApiExample { Value = CreateExample(states[0]) });
                        type.Examples.Add("Collection", new OpenApiExample { Value = CreateExample(states) });
                    }
                    else
                    {
                        type.Example = CreateExample(bodyParameterType);
                    }
                }
            }
        }
    }

    private void AddExamplesForParameters(OpenApiOperation operation, OperationFilterContext context)
    {
        AddExamplesForBodyParameter(operation, context);

        AddExamplesForQueryPamateters(operation);
    }

    private void AddExamplesForQueryPamateters(OpenApiOperation operation)
    {
        if (operation.Parameters is null)
            return;

        foreach (var parameter in operation.Parameters.OfType<OpenApiParameter>())
        {
            parameter.Examples ??= new Dictionary<string, IOpenApiExample>();

            // OData filters
            if (parameter.In == ParameterLocation.Query)
            {
                switch (parameter.Name)
                {
                    case "$filter":
                        parameter.Examples["none"] = new OpenApiExample { Value = null };
                        parameter.Examples["changed today"] = new OpenApiExample { Value = $"{nameof(ChangeTrackingEntityBase.LastChangedAt)} ge {new DateTimeOffset(DateTime.Today)}" };
                        parameter.Examples["changed by"] = new OpenApiExample { Value = $"{nameof(ChangeTrackingEntityBase.LastChangedAt)} eq Adam" };
                        break;

                    case "$orderby":
                        parameter.Examples["none"] = new OpenApiExample { Value = null };
                        parameter.Examples["id asc"] = new OpenApiExample { Value = $"{nameof(ChangeTrackingEntityBase.Id)} asc" };
                        parameter.Examples["last changed at asc"] = new OpenApiExample { Value = $"{nameof(ChangeTrackingEntityBase.LastChangedAt)} asc" };
                        parameter.Examples["last changed at desc"] = new OpenApiExample { Value = $"{nameof(ChangeTrackingEntityBase.LastChangedAt)} desc" };
                        break;

                    case "$top":
                        parameter.Examples["server default"] = new OpenApiExample { Value = null };
                        parameter.Examples["5"] = new OpenApiExample { Value = 5 };
                        parameter.Examples["10"] = new OpenApiExample { Value = 10 };
                        parameter.Examples["50"] = new OpenApiExample { Value = 50 };
                        parameter.Examples["100"] = new OpenApiExample { Value = 100 };
                        break;

                    case "$skip":
                        parameter.Examples["server default (0)"] = new OpenApiExample { Value = null };
                        parameter.Examples["5"] = new OpenApiExample { Value = 5 };
                        parameter.Examples["10"] = new OpenApiExample { Value = 10 };
                        parameter.Examples["50"] = new OpenApiExample { Value = 50 };
                        parameter.Examples["100"] = new OpenApiExample { Value = 100 };
                        break;

                    case "timestamp":
                        parameter.Examples["none (timestamp is specified in If-Match header, preferred)"] = new OpenApiExample { Value = null };
                        parameter.Examples["specified in query (if the calling application cannot send a header)"] = new OpenApiExample { Value = CreateManyExamples<byte, byte[]>(e => e.ToArray(), 8) };
                        break;

                    default:
                        break;
                }
            }

            // If-Match in delete operation
            if (parameter.In == ParameterLocation.Header && parameter.Name == "If-Match")
            {
                parameter.Examples["specified in header (preferred)"] = new OpenApiExample { Value = CreateManyExamples<byte, byte[]>(e => e.ToArray(), 8) };
                parameter.Examples["none (timestamp is specified in query, if the calling application cannot send a header)"] = new OpenApiExample { Value = null };
            }
        }
    }

    private void AddExamplesForResponses(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses is null)
            return;

        foreach (var pair in operation.Responses)
        {
            var statusCode = pair.Key;
            _ = int.TryParse(statusCode, out var statusCodeInt);

            var response = pair.Value;
            if (response.Content is null)
                continue;

            foreach (var type in response.Content.Values)
            {
                if (type.Example is null && type.Schema is OpenApiSchemaReference reference)
                {
                    try
                    {
                        var typeId = reference.Reference.Id;
                        if (typeId == "ProblemDetailsResource")
                        {
                            AddExampleForProblemResponse(statusCodeInt, response, type);
                        }
                        else
                        {
                            AddExampleForSuccessfullResponse(context, statusCodeInt, type);
                        }
                    }
                    catch { }
                }
            }
        }
    }

    private JsonNode? CreateExample(Type type)
    {
        var exampleObject = Fixture.Create(type, _specimenContext);
        return CreateExample(exampleObject);
    }

    private JsonNode? CreateExample<T>(T exampleObject)
    {
        return JsonSerializer.SerializeToNode(exampleObject, _serializerOptions);
    }

    private JsonNode? CreateManyExamples<T, U>(Func<IEnumerable<T>, U> afterCreation, int? count = default)
    {
        var exampleObject = count.HasValue ? Fixture.CreateMany<T>(count.Value) : Fixture.CreateMany<T>();
        var transformedExampleObject = afterCreation(exampleObject);
        return CreateExample(transformedExampleObject);
    }
}