using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using RESTworld.Business.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Authorization;

/// <summary>
/// This class can be used as a base for your CRUD authorization handlers.
/// Use it if you have a <see cref="ICrudServiceBase{TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/> and want to check for authorization.
/// It does not check anything, but implements the interface.
/// This allows you to only override the methods which you need to implement.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TCreateDto">The type of the create DTO.</typeparam>
/// <typeparam name="TQueryDto">The type of the query DTO.</typeparam>
/// <typeparam name="TGetListDto">The type of the get list DTO.</typeparam>
/// <typeparam name="TGetFullDto">The type of the get full DTO.</typeparam>
/// <typeparam name="TUpdateDto">The type of the update DTO.</typeparam>
/// <seealso cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}" />
public abstract class CrudAuthorizationHandlerBase<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto> : ReadAuthorizationHandlerBase<TEntity, TQueryDto, TGetListDto, TGetFullDto>, ICrudAuthorizationHandler<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>
    where TUpdateDto : class
{
    /// <inheritdoc/>
    public virtual Task<AuthorizationResult<TEntity, TCreateDto>> HandleCreateRequestAsync(AuthorizationResult<TEntity, TCreateDto> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public virtual Task<AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>>> HandleCreateRequestAsync(AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public virtual Task<ServiceResponse<TGetFullDto>> HandleCreateResponseAsync(ServiceResponse<TGetFullDto> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);

    /// <inheritdoc/>
    public virtual Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleCreateResponseAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);

    /// <inheritdoc/>
    public virtual Task<AuthorizationResult<TEntity, long, byte[]>> HandleDeleteRequestAsync(AuthorizationResult<TEntity, long, byte[]> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public virtual Task<ServiceResponse<object>> HandleDeleteResponseAsync(ServiceResponse<object> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);

    /// <inheritdoc/>
    /// <remarks>
    /// The default implementation delegates to the update and create authorization:
    /// if the patch contains update operations, <see cref="HandleUpdateRequestAsync(AuthorizationResult{TEntity, IUpdateMultipleRequest{TUpdateDto, TEntity}}, CancellationToken)"/> is called with an empty request,
    /// and if the patch contains create operations, <see cref="HandleCreateRequestAsync(AuthorizationResult{TEntity, IReadOnlyCollection{TCreateDto}}, CancellationToken)"/> is called with the DTOs to create.
    /// Any authorization filter which is added by these methods is kept and applied to all entity reads of the patch.
    /// Override this method if you need fine grained authorization based on the single operations of the patch.
    /// </remarks>
    public virtual async Task<AuthorizationResult<TEntity, JsonPatchDocument<TCreateDto, TUpdateDto>>> HandlePatchRequestAsync(AuthorizationResult<TEntity, JsonPatchDocument<TCreateDto, TUpdateDto>> previousResult, CancellationToken cancellationToken)
    {
        if (previousResult.Status != HttpStatusCode.OK)
            return previousResult;

        var split = previousResult.Value1.Split();
        var result = previousResult;

        if (split.Updates.Count > 0)
        {
            var updateRequest = new UpdateMultipleRequest<TUpdateDto, TEntity>(Array.Empty<TUpdateDto>(), x => x);
            var updateResult = await HandleUpdateRequestAsync(result.ChangeParameter<IUpdateMultipleRequest<TUpdateDto, TEntity>>(updateRequest), cancellationToken);
            result = updateResult.ChangeParameter(result.Value1);

            if (result.Status != HttpStatusCode.OK)
                return result;
        }

        if (split.Creates.Operations.Count > 0)
        {
            var createDtos = new List<TCreateDto>();
            var createsAreInvalid = false;
            try
            {
                split.Creates.ApplyTo(createDtos, _ => createsAreInvalid = true);
            }
            catch (JsonException)
            {
                createsAreInvalid = true;
            }

            // Invalid create operations are reported as validation errors by the service, so there is nothing to authorize here.
            if (!createsAreInvalid)
            {
                var createResult = await HandleCreateRequestAsync(result.ChangeParameter<IReadOnlyCollection<TCreateDto>>(createDtos), cancellationToken);
                result = createResult.ChangeParameter(result.Value1);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The default implementation delegates to <see cref="HandleUpdateResponseAsync(ServiceResponse{IReadOnlyCollection{TGetFullDto}}, CancellationToken)"/>
    /// and <see cref="HandleCreateResponseAsync(ServiceResponse{IReadOnlyCollection{TGetFullDto}}, CancellationToken)"/>, because the response may contain updated and created entities.
    /// </remarks>
    public virtual async Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandlePatchResponseAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse, CancellationToken cancellationToken)
    {
        var response = await HandleUpdateResponseAsync(previousResponse, cancellationToken);
        response = await HandleCreateResponseAsync(response, cancellationToken);
        return response;
    }

    /// <inheritdoc/>
    public virtual Task<AuthorizationResult<TEntity, TUpdateDto>> HandleUpdateRequestAsync(AuthorizationResult<TEntity, TUpdateDto> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public virtual Task<AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>>> HandleUpdateRequestAsync(AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public virtual Task<ServiceResponse<TGetFullDto>> HandleUpdateResponseAsync(ServiceResponse<TGetFullDto> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);

    /// <inheritdoc/>
    public virtual Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleUpdateResponseAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);
}