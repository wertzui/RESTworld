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
                .CreateMap<Author, AuthorDtoV1>()
                .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
                .ReverseMap()
                .ForMember(dst => dst.FirstName, opt => opt.MapFrom(src => src.Name.Split(new[] { ' ' }, 2)[0]))
                .ForMember(dst => dst.LastName, opt => opt.MapFrom(src => src.Name.Split(new[] { ' ' }, 2)[1]));

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
