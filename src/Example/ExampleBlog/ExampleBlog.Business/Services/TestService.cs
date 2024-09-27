using AutoFixture;
using AutoMapper;
using ExampleBlog.Common.Dtos;
using ExampleBlog.Data;
using ExampleBlog.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable;
using RESTworld.Business.Authorization;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using RESTworld.Business.Services;
using RESTworld.Business.Validation.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Business.Services;

public class TestService : CrudServiceBase<BlogDatabase, TestEntity, TestDto, TestDto, TestDto, TestDto>
{
    // This array simulates a cached result which might come from a database.
    private static readonly IQueryable<TestEntity> _entities;

    private static readonly IFixture _fixture;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    static TestService()
    {
        _fixture = new Fixture();

        // AutoFixture cannot create DateOnly on its own
        _fixture.Customize<DateOnly>(composer => composer.FromFactory<DateTime>(DateOnly.FromDateTime));

        // Otherwise these will be really small
        _fixture.Customize<TimeOnly>(composer => composer.FromFactory<DateTime>(TimeOnly.FromDateTime));

        _fixture.Customize<TestEntity>(composer => composer
            .With(t => t.MyRequiredString, () => null)
            .With(t => t.PostId, () => Random.Shared.NextInt64(1, 250))
            .With(t => t.MyFlagsEnum, () => TestFlagsEnum.TestFlagA | TestFlagsEnum.TestFlagB | TestFlagsEnum.TestFlagC));

        _entities = _fixture.CreateMany<TestEntity>(100)
            .ToArray()
            .BuildMock(); // BuildMock() basically allows EF Core to query the in-memory collection.
    }

    public TestService(
        IDbContextFactory<BlogDatabase> contextFactory,
        IMapper mapper,
        IEnumerable<ICrudAuthorizationHandler<TestEntity, TestDto, TestDto, TestDto, TestDto>> authorizationHandlers,
        IValidationService<TestDto, TestDto, TestEntity>? validationService,
        IUserAccessor userAccessor,
        ILogger<TestService> logger)
        : base(contextFactory, mapper, authorizationHandlers, validationService, userAccessor, logger)
    {
    }

    protected override ValueTask<IQueryable<TestEntity>> GetDbSetForReadingAsync() => ValueTask.FromResult(_entities);

    protected override IQueryable<TestEntity> GetDbSetForUpdating(BlogDatabase context) => _entities;

    protected override Task<ServiceResponse<IReadOnlyCollection<TestDto>>> CreateInternalAsync(AuthorizationResult<TestEntity, IReadOnlyCollection<TestDto>> authorizationResult, CancellationToken cancellationToken)
    {
        var dtos = authorizationResult.Value1;

        var i = 1;
        foreach (var dto in dtos)
        {
            dto.Id = i;

            _logger.LogInformation($"Created {JsonSerializer.Serialize(dto, _jsonOptions)}");

            i++;
        }

        return Task.FromResult(ServiceResponse.FromResult(dtos));
    }

    protected override Task<ServiceResponse<TestDto>> CreateInternalAsync(AuthorizationResult<TestEntity, TestDto> authorizationResult, CancellationToken cancellationToken)
    {
        var dto = authorizationResult.Value1;

        dto.Id = 1;

        _logger.LogInformation($"Created {JsonSerializer.Serialize(dto, _jsonOptions)}");

        return Task.FromResult(ServiceResponse.FromResult(dto));
    }

    protected override Task<ServiceResponse<object>> DeleteInternalAsync(AuthorizationResult<TestEntity, long, byte[]> authorizationResult, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Deleted {authorizationResult.Value1}");

        return Task.FromResult(ServiceResponse.FromStatus<object>(System.Net.HttpStatusCode.OK));
    }

    protected override Task<ServiceResponse<IReadOnlyCollection<TestDto>>> UpdateInternalAsync(AuthorizationResult<TestEntity, IUpdateMultipleRequest<TestDto, TestEntity>> authorizationResult, CancellationToken cancellationToken)
    {
        var dtos = authorizationResult.Value1.Dtos;

        foreach (var dto in dtos)
        {
            _logger.LogInformation($"Updated {JsonSerializer.Serialize(dto, _jsonOptions)}");
        }

        return Task.FromResult(ServiceResponse.FromResult(dtos));
    }

    protected override Task<ServiceResponse<TestDto>> UpdateInternalAsync(AuthorizationResult<TestEntity, TestDto> authorizationResult, CancellationToken cancellationToken)
    {
        var dto = authorizationResult.Value1;

        _logger.LogInformation($"Updated {JsonSerializer.Serialize(dto, _jsonOptions)}");

        return Task.FromResult(ServiceResponse.FromResult(dto));
    }

    protected override Task OnGotSingleInternalAsync(AuthorizationResult<TestEntity, long> authorizationResult, TestDto dto, TestEntity entity, CancellationToken cancellationToken)
    {
        dto.BlogIds = [2, 3];
        dto.MyDictionary = _fixture.Create<Dictionary<string, DictionaryTestDto>>();
        dto.MyCollection = _fixture.CreateMany<ListTestDto>().ToList();
        dto.MyNestedObject = _fixture.Create<NestedTestDto>();
        dto.MyRequiredString = null;

        return base.OnGotSingleInternalAsync(authorizationResult, dto, entity, cancellationToken);
    }

    protected override Task OnGotListInternalAsync(AuthorizationResult<TestEntity, IGetListRequest<TestDto, TestEntity>> authorizationResult, IReadOnlyPagedCollection<TestDto> pagedCollection, CancellationToken cancellationToken)
    {
        var dtos = pagedCollection.Items;

        foreach (var dto in dtos)
        {
            dto.BlogIds = [2, 3];
            dto.MyDictionary = _fixture.Create<Dictionary<string, DictionaryTestDto>>();
            dto.MyCollection = _fixture.CreateMany<ListTestDto>().ToList();
            dto.MyNestedObject = _fixture.Create<NestedTestDto>();
            dto.MyRequiredString = null;
        }

        return base.OnGotListInternalAsync(authorizationResult, pagedCollection, cancellationToken);
    }
}