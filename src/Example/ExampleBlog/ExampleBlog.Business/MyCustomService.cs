using AutoMapper;
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
using System.Text;
using System.Threading.Tasks;

namespace ExampleBlog.Business
{
    public class MyCustomService : DbServiceBase<BlogDatabase>
    {
        private readonly IEnumerable<MyCustomAuthorizationHandler> _authorizationHandlers;

        public MyCustomService(
            IDbContextFactory<BlogDatabase> contextFactory,
            IMapper mapper,
            IUserAccessor userAccessor,
            ILogger<MyCustomService> logger,
            IEnumerable<MyCustomAuthorizationHandler> authorizationHandlers)
            : base(contextFactory, mapper, userAccessor, logger)
        {
            _authorizationHandlers = authorizationHandlers ?? throw new ArgumentNullException(nameof(authorizationHandlers));
        }

        public Task<ServiceResponse<PostWithAuthorDto>> GetPostWithAuthor(long postId)
            => TryExecuteWithAuthorizationAsync<Post, long, PostWithAuthorDto, MyCustomAuthorizationHandler>(
                postId,
                result => GetPostWithAuthorInternalAsync(result),
                (result, handler) => handler.HandleRequestAsync(result),
                (response, handler) => handler.HandleResponseAsync(response),
                _authorizationHandlers);

        private async Task<ServiceResponse<PostWithAuthorDto>> GetPostWithAuthorInternalAsync(AuthorizationResult<Post, long> result)
        {
            var postId = result.Value1;

            // Get the values from the database.
            var postTask = _contextFactory.Set<Post>()
                .WithAuthorizationFilter(result)
                .SingleOrDefaultAsync(p => p.Id == postId);
            var authorTask = _contextFactory.Set<Post>()
                .WithAuthorizationFilter(result)
                .Where(p => p.Id == postId)
                .Select(p => p.Author)
                .SingleOrDefaultAsync();

            await Task.WhenAll(postTask, authorTask);

            if (postTask.Result is null)
                return ServiceResponse.FromStatus<PostWithAuthorDto>(HttpStatusCode.NotFound);

            // Map to dto.
            var dto = _mapper.Map<PostWithAuthorDto>(postTask.Result);
            _mapper.Map(authorTask.Result, dto);

            return ServiceResponse.FromResult(dto);
        }
    }
}
