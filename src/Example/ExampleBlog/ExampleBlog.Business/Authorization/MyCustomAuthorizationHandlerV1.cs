using ExampleBlog.Common.Dtos;
using ExampleBlog.Data.Models;
using RESTworld.Business.Authorization;
using RESTworld.Business.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Business.Authorization
{
    public class MyCustomAuthorizationHandlerV1 : BasicAuthorizationHandlerBase<Post, long, PostWithAuthorDtoV1>
    {
        public override Task<AuthorizationResult<Post, long>> HandleRequestAsync(AuthorizationResult<Post, long> previousResult, CancellationToken cancellationToken)
        {
            // Normally do some user authorization logic and only apply this if the user must not see posts with id 42
            var result = previousResult.WithFilter(source => source.Where(p => p.Id != 42));
            return base.HandleRequestAsync(result, cancellationToken);
        }

        public override Task<ServiceResponse<PostWithAuthorDtoV1>> HandleResponseAsync(ServiceResponse<PostWithAuthorDtoV1> previousResponse, CancellationToken cancellationToken)
        {
            // Normally do some user authorization logic and only apply this if the user must not see authors with an even ID.
            if (previousResponse.Succeeded && previousResponse.ResponseObject.AuthorId % 2 == 0)
                previousResponse.ResponseObject.Author = null;

            return base.HandleResponseAsync(previousResponse, cancellationToken);
        }
    }
}