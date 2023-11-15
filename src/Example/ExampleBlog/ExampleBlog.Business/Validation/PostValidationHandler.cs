using ExampleBlog.Common.Dtos;
using ExampleBlog.Data;
using ExampleBlog.Data.Models;
using Microsoft.EntityFrameworkCore;
using RESTworld.Business.Validation;
using RESTworld.Business.Validation.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Business.Validation;

public class PostValidatior : ValidatorBase<PostCreateDto, PostUpdateDto, Post>
{
    private readonly IDbContextFactory<BlogDatabase> _contextFactory;

    public PostValidatior(IDbContextFactory<BlogDatabase> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public override async Task<IValidationResults> ValidateBeforeCreateAsync(PostCreateDto dto, CancellationToken cancellationToken)
    {
        var postCount = await _contextFactory.Parallel().Set<Post>().CountAsync(p => p.AuthorId == dto.AuthorId, cancellationToken);

        return ValidationResults
            .Validate(postCount < 42, nameof(PostCreateDto.AuthorId), "There must be no more than 42 posts from the same author for whatever reason.")
            .Validate(!string.Equals(dto.Headline, dto.Text, System.StringComparison.OrdinalIgnoreCase), "", "Headline and Text must be different.");
    }

    public override Task<IValidationResults> ValidateBeforeUpdateAsync(PostUpdateDto dto, Post entity, CancellationToken cancellationToken)
    {
        return Task.FromResult(ValidationResults.Validate(dto.AuthorId == entity.AuthorId, nameof(PostUpdateDto.AuthorId), "The author of a post cannot be changed."));
    }
}
