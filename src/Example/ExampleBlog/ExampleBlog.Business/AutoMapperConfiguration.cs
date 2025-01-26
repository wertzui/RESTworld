using AutoMapper;
using ExampleBlog.Common.Dtos;
using ExampleBlog.Data.Models;

namespace ExampleBlog.Business;

public static class AutoMapperConfiguration
{
    public static void ConfigureAutomapper(IMapperConfigurationExpression config)
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
            .ForMember(dst => dst.FirstName, opt => opt.MapFrom(src => src.Name == null ? null : src.Name.Split(new[] { ' ' }, 2)[0]))
            .ForMember(dst => dst.LastName, opt => opt.MapFrom(src => src.Name == null ? null : src.Name.Split(new[] { ' ' }, 2)[1]));

        config
            .CreateMap<PostCreateDto, Post>();
        config
            .CreateMap<Post, PostListDto>()
            .ForMember(dst => dst.Author, opt => opt.Ignore())
            .ForMember(dst => dst.Blog, opt => opt.Ignore()); ;
        config
            .CreateMap<Post, PostGetFullDto>()
            .ForMember(dst => dst.Author, opt => opt.Ignore())
            .ForMember(dst => dst.Blog, opt => opt.Ignore());
        config
            .CreateMap<PostUpdateDto, Post>();



        config
            .CreateMap<Post, PostWithAuthorDto>();
        config.CreateMap<Author, PostWithAuthorDto>()
            .ForMember(dst => dst.Author, opt => opt.MapFrom(src => src))
            .ForMember(dst => dst.CreatedAt, opt => opt.Ignore())
            .ForMember(dst => dst.CreatedBy, opt => opt.Ignore())
            .ForMember(dst => dst.Id, opt => opt.Ignore())
            .ForMember(dst => dst.Timestamp, opt => opt.Ignore());

        config
            .CreateMap<Post, PostWithAuthorDtoV1>();
        config.CreateMap<Author, PostWithAuthorDtoV1>()
            .ForMember(dst => dst.Author, opt => opt.MapFrom(src => src));

        config
            .CreateMap<Author, AuthorStatisticsListDto>()
            .ReverseMap();
        config
            .CreateMap<Author, AuthorStatisticsFullDto>()
            .ReverseMap();

        config
            .CreateMap<TestEntity, TestDto>()
            .ReverseMap();
    }
}