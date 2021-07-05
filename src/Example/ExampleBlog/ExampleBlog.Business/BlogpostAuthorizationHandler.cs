using ExampleBlog.Common.Dtos;
using ExampleBlog.Data.Models;
using Microsoft.Extensions.Logging;
using RESTworld.Business;
using RESTworld.Business.Abstractions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ExampleBlog.Business
{
    public class BlogpostAuthorizationHandler : ICrudAuthorizationHandler<Post, PostCreateDto, PostListDto, PostGetFullDto, PostUpdateDto>
    {
        private readonly IUserAccessor _userAccessor;
        private readonly ILogger<BlogpostAuthorizationHandler> _logger;

        public BlogpostAuthorizationHandler(IUserAccessor userAccessor, ILogger<BlogpostAuthorizationHandler> logger)
        {
            _userAccessor = userAccessor ?? throw new ArgumentNullException(nameof(userAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<AuthorizationResult<Post, PostCreateDto>> HandleCreateRequestAsync(AuthorizationResult<Post, PostCreateDto> previousResult) => Task.FromResult(previousResult);

        public Task<ServiceResponse<PostGetFullDto>> HandleCreateResponseAsync(ServiceResponse<PostGetFullDto> previousResponse) => Task.FromResult(previousResponse);

        public Task<AuthorizationResult<Post, long, byte[]>> HandleDeleteRequestAsync(AuthorizationResult<Post, long, byte[]> previousResult) => Task.FromResult(previousResult);

        public Task<ServiceResponse<object>> HandleDeleteResponseAsync(ServiceResponse<object> previousResponse) => Task.FromResult(previousResponse);

        public Task<AuthorizationResult<Post, IGetListRequest<Post>>> HandleGetListRequestAsync(AuthorizationResult<Post, IGetListRequest<Post>> previousResult) => Task.FromResult(previousResult);

        public Task<ServiceResponse<IReadOnlyPagedCollection<PostListDto>>> HandleGetListResponseAsync(ServiceResponse<IReadOnlyPagedCollection<PostListDto>> previousResponse) => Task.FromResult(previousResponse);

        public async Task<AuthorizationResult<Post, long>> HandleGetSingleRequestAsync(AuthorizationResult<Post, long> previousResult)
        {
            // This is just to illustrate how authorization handlers work.
            // In a normal environment you would use the _userAccessor and do some real authorization.
            // whenever you request an id that is also an HTTP status code, you will get back the status code as result.
            var requestedPostId = previousResult.Value1;
            if (Enum.IsDefined(typeof(HttpStatusCode), (int)requestedPostId))
            {
                _logger.LogInformation($"{nameof(BlogpostAuthorizationHandler)}.{nameof(HandleGetSingleRequestAsync)} was called with a valid HTTP status code.");
                return AuthorizationResult.FromStatus<Post, long>((HttpStatusCode)requestedPostId, requestedPostId);
            }

            return previousResult;
        }

        public async Task<ServiceResponse<PostGetFullDto>> HandleGetSingleResponseAsync(ServiceResponse<PostGetFullDto> previousResponse)
        {
            // This is just to illustrate how authorization handlers work.
            // In a normal environment you would use the _userAccessor and do some real authorization.
            // whenever you request the id 42, we report a problem
            var requestedPostId = previousResponse.ResponseObject.Id;
            if (requestedPostId == 42)
            {
                _logger.LogInformation($"{nameof(BlogpostAuthorizationHandler)}.{nameof(HandleGetSingleResponseAsync)} was called with a post which has the ID 42.");
                return ServiceResponse.FromProblem<PostGetFullDto>((HttpStatusCode)42, "This is the answer!");
            }

            return previousResponse;
        }

        public Task<AuthorizationResult<Post, PostUpdateDto>> HandleUpdateRequestAsync(AuthorizationResult<Post, PostUpdateDto> previousResult) => Task.FromResult(previousResult);

        public Task<ServiceResponse<PostGetFullDto>> HandleUpdateResponseAsync(ServiceResponse<PostGetFullDto> previousResponse) => Task.FromResult(previousResponse);
    }
}
