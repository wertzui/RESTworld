using AutoFixture;
using AutoFixture.Kernel;
using HAL.AspNetCore;
using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.OData;
using HAL.Common;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
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
using System.Text.Json.Serialization;

namespace RESTworld.AspNetCore.Swagger
{
    /// <summary>
    /// This filter adds examples for all <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>s and for the <see cref="HomeController"/> to the Open API description.
    /// </summary>
    /// <seealso cref="IOperationFilter" />
    public class SwaggerExampleOperationFilter : IOperationFilter
    {
        private static readonly Fixture _fixture = new Fixture();
        private static readonly SpecimenContext _specimenContext = new SpecimenContext(_fixture);
        private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;
        private readonly string _curieName;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;
        private readonly IODataQueryFactory _oDataQueryFactory;
        private readonly JsonSerializerOptions _serializerOptions;

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
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _curieName = options.Value.GetCurieOrDefault();

            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
            _apiExplorer = apiExplorer ?? throw new ArgumentNullException(nameof(apiExplorer));
            _oDataQueryFactory = oDataQueryFactory ?? throw new ArgumentNullException(nameof(oDataQueryFactory));

            _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };
            _serializerOptions.Converters.Add(new JsonStringEnumConverter());

            _fixture
                .Customize(new DtoBaseCustomization())
                .Customize(new ChangeTrackingDtoBaseCustomization());
        }

        /// <inheritdoc/>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Body Parameters
            AddExamplesForParameters(operation, context);

            AddExamplesForResponses(operation, context);
        }

        private static Type GetBodyParameterType(MethodInfo method)
        {
            var bodyParameter = method.GetParameters().FirstOrDefault(p => p.GetCustomAttribute<FromBodyAttribute>(true) is not null);
            return bodyParameter?.ParameterType;
        }

        private void AddExampleForProblemResponse(int statusCodeInt, OpenApiResponse response, OpenApiMediaType type)
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
                var actionContextAccessor = new SwaggerOperationActionContextAccessor(context, _httpContextAccessor);
                var linkFactory = new LinkFactory(_linkGenerator, actionContextAccessor, _apiExplorer);
                var resourceFactory = new ODataResourceFactory(linkFactory, _oDataQueryFactory);

                if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    if (controllerActionDescriptor.ControllerTypeInfo.IsGenericType
                    && controllerActionDescriptor.ControllerTypeInfo.GetGenericTypeDefinition() == typeof(CrudController<,,,,>))
                    {
                        // Examples for CRUD Controller
                        AddExampleForSucessfullCrudResponse(type, linkFactory, resourceFactory, controllerActionDescriptor);
                    }
                    else if (controllerActionDescriptor.ControllerTypeInfo == typeof(HomeController) && controllerActionDescriptor.ActionName == "Index")
                    {
                        // Example for Home Controller
                        AddExampleForSuccessfullHomeResponse(type, resourceFactory);
                    }
                    else if (responseTypeFromApiDescription.Type.IsAssignableTo(typeof(Resource)))
                    {
                        // Example for any other, unknown controller
                        AddExampleForSuccessfullUnknownResourceResponse(type, responseTypeFromApiDescription.Type, resourceFactory, linkFactory, controllerActionDescriptor);
                    }
                }

                if (type.Example is null && (type.Examples?.Count).GetValueOrDefault() == 0 && responseTypeFromApiDescription.Type.IsAssignableTo(typeof(Resource)))
                {
                    // Example for a response that does not come from a controller but is still a Resource
                    AddExampleForSuccessfullUnknownResourceResponse(type, responseTypeFromApiDescription.Type, resourceFactory, linkFactory);
                }
            }
        }

        private void AddExampleForSuccessfullUnknownResourceResponse(OpenApiMediaType type, Type resourceType, ODataResourceFactory resourceFactory, ILinkFactory linkFactory, ControllerActionDescriptor controllerActionDescriptor = null)
        {
            Resource resource;
            if (resourceType.IsGenericType)
            {
                var stateType = resourceType.GenericTypeArguments[0];
                var state = _fixture.Create(stateType, _specimenContext);
                object routeValues = null;
                if (state is DtoBase dtoBase)
                    routeValues = new { id = dtoBase.Id };

                resource = resourceFactory.CreateForGetEndpoint(state, controllerActionDescriptor?.ActionName ?? "Get", controllerActionDescriptor?.ControllerName, routeValues);
            }
            else
            {
                resource = resourceFactory.Create();
                linkFactory.AddSelfLinkTo(resource, controllerActionDescriptor?.ActionName ?? "Get", controllerActionDescriptor?.ControllerName);
            }

            type.Example = CreateExample(resource);
        }

        private void AddExampleForSucessfullCrudResponse(OpenApiMediaType type, LinkFactory linkFactory, ODataResourceFactory resourceFactory, ControllerActionDescriptor controllerActionDescriptor)
        {
            var actionName = controllerActionDescriptor.ActionName;
            var controllerName = controllerActionDescriptor.ControllerName;
            if (actionName == "GetList")
            {
                var tListDto = controllerActionDescriptor.ControllerTypeInfo.GenericTypeArguments[2];
                var embedded = Enumerable.Repeat<object>(null, 3).Select(_ => _fixture.Create(tListDto, _specimenContext)).ToList();
                var resource = resourceFactory.CreateForOdataListEndpointUsingSkipTopPaging(embedded, _ => "List", e => ((DtoBase)e).Id, _oDataQueryFactory.GetListNavigation(embedded, new ODataRawQueryOptions(), linkFactory.Create(action: actionName, controller: controllerName).Href, 3, 3, 10), new Page { CurrentPage = 2, TotalPages = 4 }, controllerName);
                type.Example = CreateExample(resource);
            }
            else if (actionName == "Get" || actionName == "Post" || actionName == "Put" || actionName == "New")
            {
                var tFullDto = controllerActionDescriptor.ControllerTypeInfo.GenericTypeArguments[3];

                if (actionName == "Get" || actionName == "New")
                {
                    // Get and New only return an object
                    var state = _fixture.Create(tFullDto, _specimenContext);
                    Resource resource;
                    if (actionName == "New")
                    {
                        if (state is DtoBase dtoBase)
                        {
                            // No Id and Timestamp when creating new resources
                            dtoBase.Id = default;
                            dtoBase.Timestamp = default;

                            // Change tracking values are set by the server, not by the client
                            if (dtoBase is ChangeTrackingDtoBase changeTrackingDtoBase)
                            {
                                changeTrackingDtoBase.CreatedAt = default;
                                changeTrackingDtoBase.CreatedBy = default;
                                changeTrackingDtoBase.LastChangedAt = default;
                                changeTrackingDtoBase.LastChangedBy = default;
                            }
                        }
                        resource = resourceFactory.CreateForGetEndpoint(state, action: "New", controller: controllerName);

                        resource.AddLink("save", linkFactory.Create("POST", action: "Post", controller: controllerName));
                    }
                    else
                    {
                        resource = resourceFactory.CreateForGetEndpoint(state, controller: controllerName, routeValues: new { id = ((DtoBase)state).Id });
                    }

                    type.Example = CreateExample(resource);
                }
                else if (actionName == "Post" || actionName == "Put")
                {
                    // Post and Put either return an object or a collection
                    var states = Enumerable.Repeat<object>(null, 3).Select(_ => _fixture.Create(tFullDto, _specimenContext)).Cast<DtoBase>().ToList();
                    var CollectionResource = resourceFactory.CreateForListEndpoint(states, _ => "List", d => d.Id, controllerName);

                    type.Examples.Add("Single Object", new OpenApiExample { Value = CreateExample((Resource)resourceFactory.CreateForGetEndpoint(states[0], controller: controllerName, routeValues: new { id = states[0].Id })) });
                    type.Examples.Add("Collection", new OpenApiExample { Value = CreateExample(resourceFactory.CreateForListEndpoint(states, _ => "List", d => d.Id, controllerName)) });
                }
            }
        }

        private void AddExamplesForBodyParameter(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.RequestBody is not null)
            {
                foreach (var type in operation.RequestBody.Content.Values)
                {
                    if (type.Example is null)
                    {
                        var bodyParameterType = GetBodyParameterType(context.MethodInfo);
                        if (bodyParameterType.IsGenericType && bodyParameterType.GetGenericTypeDefinition() == typeof(SingleObjectOrCollection<>))
                        {
                            Type stateType = bodyParameterType.GenericTypeArguments[0];
                            var states = Enumerable.Repeat<object>(null, 3).Select(_ => _fixture.Create(stateType, _specimenContext)).ToList();
                            foreach (var state in states)
                            {
                                if (state is DtoBase dtoBase)
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
            foreach (var parameter in operation.Parameters)
            {
                // OData filters
                if (parameter.In == ParameterLocation.Query)
                {
                    switch (parameter.Name)
                    {
                        case "$filter":
                            parameter.Examples["none"] = new OpenApiExample { Value = new OpenApiNull() };
                            parameter.Examples["changet today"] = new OpenApiExample { Value = new OpenApiString($"{nameof(ChangeTrackingEntityBase.LastChangedAt)} ge {new DateTimeOffset(DateTime.Today)}") };
                            parameter.Examples["changed by"] = new OpenApiExample { Value = new OpenApiString($"{nameof(ChangeTrackingEntityBase.LastChangedAt)} eq Adam") };
                            break;

                        case "$orderby":
                            parameter.Examples["none"] = new OpenApiExample { Value = new OpenApiNull() };
                            parameter.Examples["id asc"] = new OpenApiExample { Value = new OpenApiString($"{nameof(ChangeTrackingEntityBase.Id)} asc") };
                            parameter.Examples["last changed at asc"] = new OpenApiExample { Value = new OpenApiString($"{nameof(ChangeTrackingEntityBase.LastChangedAt)} asc") };
                            parameter.Examples["last changed at desc"] = new OpenApiExample { Value = new OpenApiString($"{nameof(ChangeTrackingEntityBase.LastChangedAt)} desc") };
                            break;

                        case "$top":
                            parameter.Examples["server default"] = new OpenApiExample { Value = new OpenApiNull() };
                            parameter.Examples["5"] = new OpenApiExample { Value = new OpenApiString("5") };
                            parameter.Examples["10"] = new OpenApiExample { Value = new OpenApiString("10") };
                            parameter.Examples["50"] = new OpenApiExample { Value = new OpenApiString("50") };
                            parameter.Examples["100"] = new OpenApiExample { Value = new OpenApiString("100") };
                            break;

                        case "$skip":
                            parameter.Examples["default (0)"] = new OpenApiExample { Value = new OpenApiNull() };
                            parameter.Examples["5"] = new OpenApiExample { Value = new OpenApiString("5") };
                            parameter.Examples["10"] = new OpenApiExample { Value = new OpenApiString("10") };
                            parameter.Examples["50"] = new OpenApiExample { Value = new OpenApiString("50") };
                            parameter.Examples["100"] = new OpenApiExample { Value = new OpenApiString("100") };
                            break;

                        case "timestamp":
                            parameter.Examples["none (timestamp is specified in If-Match header, preferred)"] = new OpenApiExample { Value = new OpenApiNull() };
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
                    parameter.Examples["none (timestamp is specified in query, if the calling application cannot send a header)"] = new OpenApiExample { Value = new OpenApiNull() };
                }
            }
        }

        private void AddExamplesForResponses(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var pair in operation.Responses)
            {
                var statusCode = pair.Key;
                _ = int.TryParse(statusCode, out var statusCodeInt);

                var response = pair.Value;

                foreach (var type in response.Content.Values)
                {
                    if (type.Example is null)
                    {
                        var typeId = type.Schema.Reference.Id;
                        if (typeId == "ProblemDetailsResource")
                        {
                            AddExampleForProblemResponse(statusCodeInt, response, type);
                        }
                        else
                        {
                            AddExampleForSuccessfullResponse(context, statusCodeInt, type);
                        }
                    }
                }
            }
        }

        private IOpenApiAny CreateExample(Type type)
        {
            var exampleObject = _fixture.Create(type, _specimenContext);
            return CreateExample(exampleObject);
        }

        private IOpenApiAny CreateExample<T>(Action<T> afterCreation = null)
        {
            var exampleObject = _fixture.Create<T>();
            afterCreation?.Invoke(exampleObject);
            return CreateExample(exampleObject);
        }

        private IOpenApiAny CreateExample<T>(T exampleObject)
        {
            var exampleJson = JsonSerializer.Serialize(exampleObject, _serializerOptions);
            return OpenApiAnyFactory.CreateFromJson(exampleJson);
        }

        private IOpenApiAny CreateManyExamples<T>(int? count = default)
        {
            var exampleObject = count.HasValue ? _fixture.CreateMany<T>(count.Value) : _fixture.CreateMany<T>();
            return CreateExample(exampleObject);
        }

        private IOpenApiAny CreateManyExamples<T, U>(Func<IEnumerable<T>, U> afterCreation, int? count = default)
        {
            var exampleObject = count.HasValue ? _fixture.CreateMany<T>(count.Value) : _fixture.CreateMany<T>();
            var transformedExampleObject = afterCreation(exampleObject);
            return CreateExample(transformedExampleObject);
        }
    }
}