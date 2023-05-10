using AutoFixture;
using AutoMapper;
using ExampleBlog.Common.Dtos;
using Microsoft.Extensions.Logging;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using RESTworld.Business.Services;
using RESTworld.Business.Services.Abstractions;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Business.Services
{
    public class TestService : ServiceBase, ICrudServiceBase<ConcurrentEntityBase, TestDto, TestDto, TestDto, TestDto>
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
        private readonly Fixture _fixture;

        public TestService(
            IMapper mapper,
            IUserAccessor userAccessor,
            ILogger<TestService> logger)
            : base(mapper, userAccessor, logger)
        {
            _fixture = new Fixture();

            // AutoFixture cannot create DateOnly on its own
            _fixture.Customize<DateOnly>(composer => composer.FromFactory<DateTime>(DateOnly.FromDateTime));

            // Otherwise these will be really small
            _fixture.Customize<TimeOnly>(composer => composer.FromFactory<DateTime>(TimeOnly.FromDateTime));
        }

        public Task<ServiceResponse<TestDto>> CreateAsync(TestDto dto, CancellationToken cancellationToken)
        {
            dto.Id = 1;

            _logger.LogInformation($"Created {JsonSerializer.Serialize(dto, _jsonOptions)}");

            return Task.FromResult(ServiceResponse.FromResult(dto));
        }

        public Task<ServiceResponse<IReadOnlyCollection<TestDto>>> CreateAsync(IReadOnlyCollection<TestDto> dtos, CancellationToken cancellationToken)
        {
            var id = 1;
            foreach (var dto in dtos)
            {
                dto.Id = id;
                id++;
            }

            _logger.LogInformation($"Created {JsonSerializer.Serialize(dtos, _jsonOptions)}");

            return Task.FromResult(ServiceResponse.FromResult(dtos));
        }

        public Task<ServiceResponse<object>> DeleteAsync(long id, byte[] timestamp, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Deleted {id}");

            return Task.FromResult(ServiceResponse.FromStatus<object>(System.Net.HttpStatusCode.OK));
        }

        public Task<ServiceResponse<IReadOnlyPagedCollection<TestDto>>> GetListAsync(IGetListRequest<ConcurrentEntityBase> request, CancellationToken cancellationToken)
        {
            var totalCount = 10;
            var dtos = _fixture.CreateMany<TestDto>(totalCount).ToList();

            IReadOnlyPagedCollection<TestDto> page = new ReadOnlyPagedCollection<TestDto>(dtos, totalCount);
            return Task.FromResult(ServiceResponse.FromResult(page));
        }

        public Task<ServiceResponse<TestDto>> GetSingleAsync(long id, CancellationToken cancellationToken)
        {
            var dto = _fixture.Create<TestDto>();
            dto.Id = id;
            dto.MyNullCollection = null;
            dto.BlogIds = new HashSet<long> { 2, 3 };

            return Task.FromResult(ServiceResponse.FromResult(dto));
        }

        public Task<ServiceResponse<TestDto>> UpdateAsync(TestDto dto, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Updated {JsonSerializer.Serialize(dto, _jsonOptions)}");

            return Task.FromResult(ServiceResponse.FromResult(dto));
        }

        public Task<ServiceResponse<IReadOnlyCollection<TestDto>>> UpdateAsync(IUpdateMultipleRequest<TestDto, ConcurrentEntityBase> request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Updated {JsonSerializer.Serialize(request.Dtos, _jsonOptions)}");

            return Task.FromResult(ServiceResponse.FromResult(request.Dtos));
        }
    }
}
