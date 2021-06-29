using AutoMapper;
using ExampleBlog.Common.Dtos;
using ExampleBlog.Data.Models;

namespace ExampleBlog.Business
{
    public class AutoMapperConfiguration
    {
        public void ConfigureAutomapper(IMapperConfigurationExpression config)
        {
            config
                .CreateMap<Blog, BlogDto>()
                .ReverseMap();

            config
                .CreateMap<Author, AuthorDto>()
                .ReverseMap();

            config
                .CreateMap<PostCreateDto, Post>();
            config
                .CreateMap<Post, PostListDto>();
            config
                .CreateMap<Post, PostGetFullDto>();
            config
                .CreateMap<PostUpdateDto, Post>();
        }
    }
}
