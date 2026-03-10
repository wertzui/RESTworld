using AutoMapper;
using ExampleBlog.Common.Dtos;
using ExampleBlog.Data.Models;
using RESTworld.Business.Mapping.AutoMapper;
using System;

namespace ExampleBlog.Business.Mapping;

public class AuthorPostMapperV1 : ReadAutoMapper<Author, PostWithAuthorDtoV1, PostWithAuthorDtoV1, PostWithAuthorDtoV1>
{
    private readonly IMapper _mapper;

    public AuthorPostMapperV1(IMapper mapper)
        : base(mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public void AddToGetFull(Author author, PostWithAuthorDtoV1 getFullDto) => _mapper.Map(author, getFullDto);
}
