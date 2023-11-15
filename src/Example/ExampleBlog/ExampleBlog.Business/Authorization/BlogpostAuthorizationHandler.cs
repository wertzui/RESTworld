using ExampleBlog.Common.Dtos;
using ExampleBlog.Data.Models;
using Microsoft.Extensions.Logging;
using RESTworld.Business.Authorization;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Business.Authorization;

public class BlogpostAuthorizationHandler : CrudAuthorizationHandlerBase<Post, PostCreateDto, PostListDto, PostGetFullDto, PostUpdateDto>
{
    private readonly ILogger<BlogpostAuthorizationHandler> _logger;

    public BlogpostAuthorizationHandler(ILogger<BlogpostAuthorizationHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override Task<AuthorizationResult<Post, IGetListRequest<Post>>> HandleGetListRequestAsync(AuthorizationResult<Post, IGetListRequest<Post>> previousResult, CancellationToken cancellationToken)
    {
        // This is just to illustrate how authorization handlers work.
        // In a normal environment you would use the current user (probably through UserIsAuthorizedCrudAuthorizationHandler) and do some real authorization.
        // We only return posts with an even ID.
        return Task.FromResult(previousResult.WithFilter(source => source.Where(p => p.Id % 2 == 0)));
    }

    public override Task<AuthorizationResult<Post, long>> HandleGetSingleRequestAsync(AuthorizationResult<Post, long> previousResult, CancellationToken cancellationToken)
    {
        // This is just to illustrate how authorization handlers work.
        // In a normal environment you would use the current user (probably through UserIsAuthorizedCrudAuthorizationHandler) and do some real authorization.
        // Whenever you request an id that is also an HTTP status code, you will get back the status code as result.
        var requestedPostId = previousResult.Value1;
        if (Enum.IsDefined(typeof(HttpStatusCode), (int)requestedPostId))
        {
            _logger.LogInformation($"{nameof(BlogpostAuthorizationHandler)}.{nameof(HandleGetSingleRequestAsync)} was called with a valid HTTP status code.");
            return Task.FromResult(AuthorizationResult.FromStatus<Post, long>((HttpStatusCode)requestedPostId, requestedPostId));
        }

        return Task.FromResult(previousResult);
    }

    public override Task<ServiceResponse<PostGetFullDto>> HandleGetSingleResponseAsync(ServiceResponse<PostGetFullDto> previousResponse, CancellationToken cancellationToken)
    {
        // This is just to illustrate how authorization handlers work.
        // In a normal environment you would use the current user (probably through UserIsAuthorizedCrudAuthorizationHandler) and do some real authorization.
        // Instead of the post with the ID 42, we report a problem.
        var requestedPostId = previousResponse.ResponseObject?.Id;
        if (requestedPostId == 42)
        {
            _logger.LogInformation($"{nameof(BlogpostAuthorizationHandler)}.{nameof(HandleGetSingleResponseAsync)} was called with a post which has the ID 42.");
            return Task.FromResult(ServiceResponse.FromProblem<PostGetFullDto>((HttpStatusCode)42, "This is the answer!"));
        }

        return Task.FromResult(previousResponse);
    }
}