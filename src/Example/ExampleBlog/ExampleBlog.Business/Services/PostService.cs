﻿using AutoMapper;
using ExampleBlog.Common.Dtos;
using ExampleBlog.Data;
using ExampleBlog.Data.Models;
using HAL.Common.Binary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RESTworld.Business.Authorization;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using RESTworld.Business.Services;
using RESTworld.Business.Validation.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Business.Services;

public class PostService : CrudServiceBase<BlogDatabase, Post, PostCreateDto, PostListDto, PostGetFullDto, PostUpdateDto>
{
    public PostService(
        IDbContextFactory<BlogDatabase> contextFactory,
        IMapper mapper,
        IEnumerable<ICrudAuthorizationHandler<Post, PostCreateDto, PostListDto, PostGetFullDto, PostUpdateDto>> authorizationHandlers,
        IValidationService<PostCreateDto, PostUpdateDto, Post> validationService,
        IUserAccessor userAccessor,
        ILogger<CrudServiceBase<BlogDatabase, Post, PostCreateDto, PostListDto, PostGetFullDto, PostUpdateDto>> logger)
        : base(contextFactory, mapper, authorizationHandlers, validationService, userAccessor, logger)
    {
    }

    protected override async Task<ServiceResponse<PostGetFullDto>> GetSingleInternalAsync(AuthorizationResult<Post, long> authorizationResult, CancellationToken cancellationToken)
    {
        var result = await base.GetSingleInternalAsync(authorizationResult, cancellationToken);
        if (result.Succeeded && result.ResponseObject is not null)
        {
            // The attachment may be a file that is read from disc.
            result.ResponseObject.Attachement = GetAttachement(result.ResponseObject.Id);

            // The image might be stored on another URL so we just return the URL.
            result.ResponseObject.Image = await GetImageAsync(result.ResponseObject.Headline, cancellationToken);
        }

        return result;
    }

    protected override async Task<ServiceResponse<PostGetFullDto>> UpdateInternalAsync(AuthorizationResult<Post, PostUpdateDto> authorizationResult, CancellationToken cancellationToken)
    {
        var result = await base.UpdateInternalAsync(authorizationResult, cancellationToken);
        if (result.Succeeded)
        {
            var dto = authorizationResult.Value1;

            SaveImage(dto.Image);
            SaveAttachement(dto.Attachement);

            if (result.ResponseObject is not null)
            {
                result.ResponseObject.Image = await GetImageAsync(result.ResponseObject.Headline, cancellationToken);
                result.ResponseObject.Attachement = GetAttachement(result.ResponseObject.Id);
            }
        }

        return result;
    }

    protected override async Task<ServiceResponse<IReadOnlyCollection<PostGetFullDto>>> UpdateInternalAsync(AuthorizationResult<Post, IUpdateMultipleRequest<PostUpdateDto, Post>> authorizationResult, CancellationToken cancellationToken)
    {
        var result = await base.UpdateInternalAsync(authorizationResult, cancellationToken);
        if (result.Succeeded)
        {
            foreach (var requestDto in authorizationResult.Value1.Dtos)
            {
                SaveImage(requestDto.Image);
                SaveAttachement(requestDto.Attachement);
            }
            if (result.ResponseObject is not null)
            {
                foreach (var resultDto in result.ResponseObject)
                {
                    resultDto.Image = await GetImageAsync(resultDto.Headline, cancellationToken);
                    resultDto.Attachement = GetAttachement(resultDto.Id);
                }
            }
        }

        return result;
    }

    protected override async Task<ServiceResponse<PostGetFullDto>> CreateInternalAsync(AuthorizationResult<Post, PostCreateDto> authorizationResult, CancellationToken cancellationToken)
    {
        var result = await base.CreateInternalAsync(authorizationResult, cancellationToken);
        if (result.Succeeded)
        {
            var dto = authorizationResult.Value1;

            SaveImage(dto.Image);
            SaveAttachement(dto.Attachement);

            if (result.ResponseObject is not null)
            {
                result.ResponseObject.Image = await GetImageAsync(result.ResponseObject.Headline, cancellationToken);
                result.ResponseObject.Attachement = GetAttachement(result.ResponseObject.Id);
            }
        }

        return result;
    }

    protected override async Task<ServiceResponse<IReadOnlyCollection<PostGetFullDto>>> CreateInternalAsync(AuthorizationResult<Post, IReadOnlyCollection<PostCreateDto>> authorizationResult, CancellationToken cancellationToken)
    {
        var result = await base.CreateInternalAsync(authorizationResult, cancellationToken);
        if (result.Succeeded)
        {
            foreach (var requestDto in authorizationResult.Value1)
            {
                SaveImage(requestDto.Image);
                SaveAttachement(requestDto.Attachement);
            }

            if (result.ResponseObject is not null)
            {
                foreach (var resultDto in result.ResponseObject)
                {
                    resultDto.Image = await GetImageAsync(resultDto.Headline, cancellationToken);
                    resultDto.Attachement = GetAttachement(resultDto.Id);
                }
            }
        }

        return result;
    }

    private void SaveImage(HalFile? image)
    {
        if (image?.Content is not null)
        {
            // Upload the image to a cloud service provider.
        }
    }

    private void SaveAttachement(HalFile? attachement)
    {
        if (attachement?.Content is not null)
        {
            // Store the file on disc.
        }
    }

    private async Task<HalFile> GetImageAsync(string headline, CancellationToken cancellationToken)
    {
        var url = $"https://dummyimage.com/120x40/000/fff&text={headline.Replace(" ", "_")}";
        var client = new HttpClient();
        var bytes = await client.GetByteArrayAsync(url, cancellationToken);
        var image = new HalFile("image/png", bytes);

        return image;
    }

    private HalFile GetAttachement(long id)
    {
        using var stream = new MemoryStream();

        // Simulate a file on disk
        using var writer = new StreamWriter(stream);
        writer.AutoFlush = true;
        writer.WriteLine($"This is the attachment of the post {id}.");
        stream.Seek(0, SeekOrigin.Begin);

        // Simulate reading a file
        var bytes = stream.ToArray();
        return new HalFile("text/plain", bytes);
    }
}
