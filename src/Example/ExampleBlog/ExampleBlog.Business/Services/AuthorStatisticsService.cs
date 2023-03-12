using AutoMapper;
using ExampleBlog.Common.Dtos;
using ExampleBlog.Data;
using ExampleBlog.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RESTworld.Business.Authorization;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Business.Services
{
    public class AuthorStatisticsService : ReadServiceBase<BlogDatabase, Author, AuthorStatisticsListDto, AuthorStatisticsFullDto>
    {
        public AuthorStatisticsService(
            IDbContextFactory<BlogDatabase> contextFactory,
            IMapper mapper,
            IEnumerable<IReadAuthorizationHandler<Author, AuthorStatisticsListDto, AuthorStatisticsFullDto>> authorizationHandlers,
            IUserAccessor userAccessor,
            ILogger<AuthorStatisticsService> logger) :
            base(contextFactory, mapper, authorizationHandlers, userAccessor, logger)
        {
        }

        protected override async Task OnGotSingleInternalAsync(AuthorizationResult<Author, long> authorizationResult, AuthorStatisticsFullDto dto, Author entity, CancellationToken cancellationToken)
        {
            var authorId = entity.Id;
            var postDates = await _contextFactory.Parallel().Set<Post>()
                .Where(p => p.AuthorId == authorId && p.CreatedAt.HasValue)
                .Select(p => p.CreatedAt!.Value)
                .ToListAsync(cancellationToken);

            dto.TotalPosts = postDates.Count;
            dto.PostsPerMonth = postDates.GroupBy(p => new DateTimeOffset(p.Year, p.Month, 1, 0, 0, 0, p.Offset)).ToDictionary(g => g.Key, g => g.LongCount());
            dto.PostsPerYear = postDates.GroupBy(p => new DateTimeOffset(p.Year, 1, 1, 0, 0, 0, p.Offset)).ToDictionary(g => g.Key, g => g.LongCount());
        }
    }
}