using ExampleBlog.Common.Dtos;
using ExampleBlog.Data.Models;
using RESTworld.Business.Mapping.Mapperly;
using Riok.Mapperly.Abstractions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace ExampleBlog.Business.Mapping;

[Mapper]
public partial class BlogMapper : CrudMapperlyMapperBase<Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto>
{
    // When creating an entry, everything on ChangeTrackingEntityBase should be ignored.
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public override partial Blog MapCreateToEntity(BlogCreateDto createDto);

    // Foreign Keys should be ignored
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public override partial BlogGetFullDto MapEntityToFull(Blog entity);

    public override partial Expression<Func<Blog, BlogGetFullDto>> MapEntityToFullExpression();

    // Foreign Keys should be ignored
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public override partial BlogGetFullDto MapQueryToFull(BlogQueryDto entity);

    public override partial IQueryable<BlogGetFullDto> MapQueryToFullQueryable(IQueryable<BlogQueryDto> entities);

    // Foreign Keys should be ignored
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public override partial BlogGetListDto MapQueryToList(BlogQueryDto queryDto);

    public override partial IQueryable<BlogGetListDto> MapQueryToListQueryable(IQueryable<BlogQueryDto> queryDtos);

    public override partial IQueryable<BlogQueryDto> MapEntityToQuery(IQueryable<Blog> entities);

    // When updating an entry, everything on ChangeTrackingEntityBase except the Id should be ignored.
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public override partial void MapUpdateToEntity(BlogUpdateDto updateDto, Blog entity);
}