using ExampleBlog.Common.Dtos;
using ExampleBlog.Data.Models;
using RESTworld.Business.Mapping.Mapperly;
using Riok.Mapperly.Abstractions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace ExampleBlog.Business.Mapping;

[Mapper]
public partial class PostMapper : CrudMapperlyMapperBase<Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto>
{
    [MapperIgnoreSource(nameof(PostCreateDto.Attachement))]
    [MapperIgnoreSource(nameof(PostCreateDto.Image))]
    [MapperIgnoreSource(nameof(PostCreateDto.Author))]
    [MapperIgnoreSource(nameof(PostCreateDto.Blog))]
    [MapperIgnoreTarget(nameof(Post.Author))]
    [MapperIgnoreTarget(nameof(Post.Blog))]
    [MapperIgnoreTarget(nameof(Post.Id))]
    [MapperIgnoreTarget(nameof(Post.CreatedAt))]
    [MapperIgnoreTarget(nameof(Post.CreatedBy))]
    [MapperIgnoreTarget(nameof(Post.LastChangedAt))]
    [MapperIgnoreTarget(nameof(Post.LastChangedBy))]
    [MapperIgnoreTarget(nameof(Post.Timestamp))]
    public override partial Post MapCreateToEntity(PostCreateDto createDto);

    public override partial Expression<Func<Post, PostGetFullDto>> MapEntityToFullExpression();

    [MapperIgnoreSource(nameof(Post.Author))]
    [MapperIgnoreSource(nameof(Post.Blog))]
    [MapperIgnoreTarget(nameof(PostGetFullDto.Attachement))]
    [MapperIgnoreTarget(nameof(PostGetFullDto.Image))]
    [MapperIgnoreTarget(nameof(PostGetFullDto.Author))]
    [MapperIgnoreTarget(nameof(PostGetFullDto.Blog))]
    public override partial PostGetFullDto MapEntityToFull(Post entity);

    [MapperIgnoreSource(nameof(Post.State))]
    [MapperIgnoreSource(nameof(Post.Text))]
    public override partial PostListDto MapQueryToList(PostQueryDto entity);

    // Needs to be here to allow mapping of Post.Author
    [MapperIgnoreSource(nameof(Author.Posts))]
    private partial AuthorDto MapToAuthorDto(Author source);

    // Needs to be here to allow mapping of Post.Blog
    [MapperIgnoreSource(nameof(Blog.Posts))]
    private partial BlogGetFullDto MapToBlogDto(Blog source);

    public override partial IQueryable<PostQueryDto> MapEntityToQueryQueryable(IQueryable<Post> entities);

    [MapperIgnoreSource(nameof(PostUpdateDto.Id))]
    [MapperIgnoreSource(nameof(PostUpdateDto.Timestamp))]
    [MapperIgnoreSource(nameof(PostUpdateDto.Attachement))]
    [MapperIgnoreSource(nameof(PostUpdateDto.Image))]
    [MapperIgnoreTarget(nameof(Post.Author))]
    [MapperIgnoreTarget(nameof(Post.Blog))]
    [MapperIgnoreTarget(nameof(Post.Id))]
    [MapperIgnoreTarget(nameof(Post.CreatedAt))]
    [MapperIgnoreTarget(nameof(Post.CreatedBy))]
    [MapperIgnoreTarget(nameof(Post.LastChangedAt))]
    [MapperIgnoreTarget(nameof(Post.LastChangedBy))]
    [MapperIgnoreTarget(nameof(Post.Timestamp))]
    public override partial void MapUpdateToEntity(PostUpdateDto updateDto, Post entity);

    public override partial IQueryable<PostListDto> MapQueryToListQueryable(IQueryable<PostQueryDto> queryDtos);

    [MapperIgnoreTarget(nameof(PostUpdateDto.Attachement))]
    [MapperIgnoreTarget(nameof(PostUpdateDto.Image))]
    public override partial PostGetFullDto MapQueryToFull(PostQueryDto entity);

    public override partial IQueryable<PostGetFullDto> MapQueryToFullQueryable(IQueryable<PostQueryDto> entities);
}