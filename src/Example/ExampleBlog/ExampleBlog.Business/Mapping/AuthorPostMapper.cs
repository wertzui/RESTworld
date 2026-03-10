using AutoMapper;
using ExampleBlog.Common.Dtos;
using ExampleBlog.Data.Models;
using RESTworld.Business.Mapping.AutoMapper;
using System;

namespace ExampleBlog.Business.Mapping;

public class AuthorPostMapper : ReadAutoMapper<Author, PostWithAuthorDto, PostWithAuthorDto, PostWithAuthorDto>
{
    private readonly IMapper _mapper;

    public AuthorPostMapper(IMapper mapper)
        : base(mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public void AddToGetFull(Author author, PostWithAuthorDto getFullDto) => _mapper.Map(author, getFullDto);
}