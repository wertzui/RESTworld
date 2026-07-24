using ExampleBlog.Business.Authorization;
using ExampleBlog.Common.Dtos;
using ExampleBlog.Data;
using ExampleBlog.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RESTworld.Business.Authorization;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Mapping;
using RESTworld.Business.Mapping.Exceptions;
using RESTworld.Business.Models;
using RESTworld.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Business.Services;

public class MyCustomService : DbServiceBase<BlogDatabase>
{
    private readonly IReadMapper<Post, PostWithAuthorDto, PostWithAuthorDto, PostWithAuthorDto> _postMapper;
    private readonly IReadMapper<Author, AuthorDto, AuthorDto, AuthorDto> _authorMapper;
    private readonly IEnumerable<MyCustomAuthorizationHandler> _authorizationHandlers;

    public MyCustomService(
        IDbContextFactory<BlogDatabase> contextFactory,
        IReadMapper<Post, PostWithAuthorDto, PostWithAuthorDto, PostWithAuthorDto> postMapper,
        IReadMapper<Author, AuthorDto, AuthorDto, AuthorDto> authorMapper,
        IUserAccessor userAccessor,
        ILogger<MyCustomService> logger,
        IEnumerable<MyCustomAuthorizationHandler> authorizationHandlers)
        : base(contextFactory, [.. ExceptionTranslatorFactory.CreateExceptionTranslators(postMapper, contextFactory), .. ExceptionTranslatorFactory.CreateExceptionTranslators(authorMapper, contextFactory)], userAccessor, logger)
    {
        _postMapper = postMapper ?? throw new ArgumentNullException(nameof(postMapper));
        _authorMapper = authorMapper ?? throw new ArgumentNullException(nameof(authorMapper));
        _authorizationHandlers = authorizationHandlers ?? throw new ArgumentNullException(nameof(authorizationHandlers));
    }

    public Task<ServiceResponse<PostWithAuthorDto>> GetPostWithAuthorAsync(long postId, CancellationToken cancellationToken)
        => TryExecuteWithAuthorizationAsync<Post, long, PostWithAuthorDto, MyCustomAuthorizationHandler>(
            postId,
            (result, token) => GetPostWithAuthorInternalAsync(result, token),
            (result, handler, token) => handler.HandleRequestAsync(result, token),
            (response, handler, token) => handler.HandleResponseAsync(response, token),
            _authorizationHandlers,
            cancellationToken);

    private async Task<ServiceResponse<PostWithAuthorDto>> GetPostWithAuthorInternalAsync(AuthorizationResult<Post, long> result, CancellationToken cancellationToken)
    {
        var postId = result.Value1;

        // Get the values from the database.
        var postSet = _contextFactory.Parallel().Set<Post>()
            .WithAuthorizationFilter(result);
        var postTask = _postMapper.MapEntityToFullQueryable(postSet)
            .SingleOrDefaultAsync(p => p.Id == postId, cancellationToken);
        var authorTask = _authorMapper.MapEntityToFullQueryable(postSet
            .Where(p => p.Id == postId)
            .Select(p => p.Author))
            .SingleOrDefaultAsync(cancellationToken);

        await Task.WhenAll(postTask, authorTask);

        if (postTask.Result is null)
            return ServiceResponse.FromStatus<PostWithAuthorDto>(HttpStatusCode.NotFound);

        // Map to DTO.
        var dto = postTask.Result;

        if (authorTask.Result is not null)
            dto.Author = authorTask.Result;

        return ServiceResponse.FromResult(dto);
    }
}
