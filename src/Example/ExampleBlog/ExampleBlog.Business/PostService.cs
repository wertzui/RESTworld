using AutoMapper;
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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ExampleBlog.Business
{
    public class PostService : CrudServiceBase<BlogDatabase, Post, PostCreateDto, PostListDto, PostGetFullDto, PostUpdateDto>
    {
        public PostService(IDbContextFactory<BlogDatabase> contextFactory, IMapper mapper, IEnumerable<ICrudAuthorizationHandler<Post, PostCreateDto, PostListDto, PostGetFullDto, PostUpdateDto>> authorizationHandlers, IUserAccessor userAccessor, ILogger<CrudServiceBase<BlogDatabase, Post, PostCreateDto, PostListDto, PostGetFullDto, PostUpdateDto>> logger) : base(contextFactory, mapper, authorizationHandlers, userAccessor, logger)
        {
        }

        protected override async Task<ServiceResponse<PostGetFullDto>> GetSingleInternalAsync(AuthorizationResult<Post, long> authorizationResult)
        {
            var result = await base.GetSingleInternalAsync(authorizationResult);
            if (result.Succeeded)
            {
                // The attachement may be a file that is read from disc.
                result.ResponseObject.Attachement = GetAttachement(result.ResponseObject.Id);

                // The image might be stored on another URL so we just return the url.
                result.ResponseObject.Image = GetImage(result.ResponseObject.Headline);
            }

            return result;
        }

        protected override async Task<ServiceResponse<PostGetFullDto>> UpdateInternalAsync(AuthorizationResult<Post, PostUpdateDto> authorizationResult)
        {
            var result = await base.UpdateInternalAsync(authorizationResult);
            if (result.Succeeded)
            {
                var dto = authorizationResult.Value1;

                SaveImage(dto.Image);
                SaveAttachement(dto.Attachement);

                result.ResponseObject.Image = GetImage(result.ResponseObject.Headline);
                result.ResponseObject.Attachement = GetAttachement(result.ResponseObject.Id);
            }

            return result;
        }

        protected override async Task<ServiceResponse<IReadOnlyCollection<PostGetFullDto>>> UpdateInternalAsync(AuthorizationResult<Post, IUpdateMultipleRequest<PostUpdateDto, Post>> authorizationResult)
        {
            var result = await base.UpdateInternalAsync(authorizationResult);
            if (result.Succeeded)
            {
                foreach (var requestDto in authorizationResult.Value1.Dtos)
                {
                    SaveImage(requestDto.Image);
                    SaveAttachement(requestDto.Attachement);
                }

                foreach (var resultDto in result.ResponseObject)
                {
                    resultDto.Image = GetImage(resultDto.Headline);
                    resultDto.Attachement = GetAttachement(resultDto.Id);
                }
            }

            return result;
        }

        protected override async Task<ServiceResponse<PostGetFullDto>> CreateInternalAsync(AuthorizationResult<Post, PostCreateDto> authorizationResult)
        {
            var result = await base.CreateInternalAsync(authorizationResult);
            if (result.Succeeded)
            {
                var dto = authorizationResult.Value1;

                SaveImage(dto.Image);
                SaveAttachement(dto.Attachement);

                result.ResponseObject.Image = GetImage(result.ResponseObject.Headline);
                result.ResponseObject.Attachement = GetAttachement(result.ResponseObject.Id);
            }

            return result;
        }

        protected override async Task<ServiceResponse<IReadOnlyCollection<PostGetFullDto>>> CreateInternalAsync(AuthorizationResult<Post, IReadOnlyCollection<PostCreateDto>> authorizationResult)
        {
            var result = await base.CreateInternalAsync(authorizationResult);
            if (result.Succeeded)
            {
                foreach (var requestDto in authorizationResult.Value1)
                {
                    SaveImage(requestDto.Image);
                    SaveAttachement(requestDto.Attachement);
                }

                foreach (var resultDto in result.ResponseObject)
                {
                    resultDto.Image = GetImage(resultDto.Headline);
                    resultDto.Attachement = GetAttachement(resultDto.Id);
                }
            }

            return result;
        }

        private void SaveImage(HalFile image)
        {
            if (image?.Content is not null)
            {
                // Upload the image to a cloud service provider.
            }
        }

        private void SaveAttachement(HalFile attachement)
        {
            if (attachement?.Content is not null)
            {
                // Store the file on disc.
            }
        }

        private HalFile GetImage(string headline) => new HalFile($"https://dummyimage.com/600x400/000/fff&text={headline.Replace(" ", "_")}");

        private HalFile GetAttachement(long id)
        {
            using var stream = new MemoryStream();

            // Simulate a file on disk
            using var writer = new StreamWriter(stream);
            writer.AutoFlush = true;
            writer.WriteLine($"This is the attachement of the post {id}.");
            stream.Seek(0, SeekOrigin.Begin);

            // Simulate reading a file
            var bytes = stream.ToArray();
            return new HalFile("text/plain", bytes);
        }
    }
}
