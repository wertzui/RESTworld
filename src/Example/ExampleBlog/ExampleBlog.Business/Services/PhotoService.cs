using AutoMapper;
using Microsoft.Extensions.Logging;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using RESTworld.Business.Services;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Business.Services
{
    public class PhotoService : ServiceBase, IPhotoService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PhotoService(IHttpClientFactory httpClientFactory,
            IMapper mapper,
            IUserAccessor userAccessor,
            ILogger<PhotoService> logger)
            : base(mapper, userAccessor, logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public Task<ServiceResponse<Stream>> GetPhotoAsync(string email, CancellationToken cancellationToken)
            => TryExecuteAsync(token => GetPhotoInternalAsync(email, token), cancellationToken);

        private async Task<ServiceResponse<Stream>> GetPhotoInternalAsync(string email, CancellationToken cancellationToken)
        {
            email ??= "";

            if (email.Contains('@'))
                email = email[..email.IndexOf('@')];

            email = Uri.EscapeDataString(email);

            var url = $"https://dummyimage.com/100x100/000/fff&text={email}";

            var client = _httpClientFactory.CreateClient();
            var stream = await client.GetStreamAsync(url);

            return ServiceResponse.FromResult(stream);
        }
    }
}