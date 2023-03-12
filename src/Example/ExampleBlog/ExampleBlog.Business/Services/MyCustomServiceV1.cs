using AutoMapper;
using ExampleBlog.Business.Authorization;
using ExampleBlog.Common.Dtos;
using ExampleBlog.Data;
using ExampleBlog.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RESTworld.Business.Authorization;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using RESTworld.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Business.Services
{
    public class MyCustomServiceV1 : DbServiceBase<BlogDatabase>
    {
        private readonly IEnumerable<MyCustomAuthorizationHandlerV1> _authorizationHandlers;

        public MyCustomServiceV1(
            IDbContextFactory<BlogDatabase> contextFactory,
            IMapper mapper,
            IUserAccessor userAccessor,
            ILogger<MyCustomService> logger,
            IEnumerable<MyCustomAuthorizationHandlerV1> authorizationHandlers)
            : base(contextFactory, mapper, userAccessor, logger)
        {
            _authorizationHandlers = authorizationHandlers ?? throw new ArgumentNullException(nameof(authorizationHandlers));
        }

        public Task<ServiceResponse<PostWithAuthorDtoV1>> GetPostWithAuthor(long postId, CancellationToken cancellationToken)
            => TryExecuteWithAuthorizationAsync<Post, long, PostWithAuthorDtoV1, MyCustomAuthorizationHandlerV1>(
                postId,
                (result, token) => GetPostWithAuthorInternalAsync(result, token),
                (result, handler, token) => handler.HandleRequestAsync(result, token),
                (response, handler, token) => handler.HandleResponseAsync(response, token),
                _authorizationHandlers,
                cancellationToken);

        private async Task<ServiceResponse<PostWithAuthorDtoV1>> GetPostWithAuthorInternalAsync(AuthorizationResult<Post, long> result, CancellationToken cancellationToken)
        {
            var postId = result.Value1;

            // Get the values from the database.
            var postTask = _contextFactory.Parallel().Set<Post>()
                .WithAuthorizationFilter(result)
                .SingleOrDefaultAsync(p => p.Id == postId, cancellationToken);
            var authorTask = _contextFactory.Parallel().Set<Post>()
                .WithAuthorizationFilter(result)
                .Where(p => p.Id == postId)
                .Select(p => p.Author)
                .SingleOrDefaultAsync(cancellationToken);

            await Task.WhenAll(postTask, authorTask);

            if (postTask.Result is null)
                return ServiceResponse.FromStatus<PostWithAuthorDtoV1>(HttpStatusCode.NotFound);

            // Map to DTO.
            var dto = _mapper.Map<PostWithAuthorDtoV1>(postTask.Result);
            _mapper.Map(authorTask.Result, dto);

            return ServiceResponse.FromResult(dto);
        }
    }
}
