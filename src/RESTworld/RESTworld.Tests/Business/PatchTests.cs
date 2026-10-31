using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTworld.Business.Authorization;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using RESTworld.Business.Services.Abstractions;
using RESTworld.Testing;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Tests.Business;

[TestClass]
public sealed class PatchTests
{
    private const long ExistingId = 1;

    [TestMethod]
    public async Task Patch_that_only_creates_checks_only_create_authorization()
    {
        // Arrange
        var (service, handler, _) = CreateSut();
        var patch = CreatePatch().Add("/-", new TestDto { Foo = "created" });

        // Act
        var response = await service.PatchAsync(patch, CancellationToken.None);

        // Assert
        Assert.IsTrue(response.Succeeded, response.ProblemDetails);
        Assert.AreEqual(1, handler.CreateRequests);
        Assert.AreEqual(0, handler.UpdateRequests);
        Assert.AreEqual(1, response.ResponseObject.Count);
        Assert.AreEqual("created", response.ResponseObject.Single().Foo);
    }

    [TestMethod]
    public async Task Patch_that_only_updates_checks_only_update_authorization()
    {
        // Arrange
        var (service, handler, _) = CreateSut();
        var patch = CreatePatch().Replace($"/{ExistingId}/foo", "updated");

        // Act
        var response = await service.PatchAsync(patch, CancellationToken.None);

        // Assert
        Assert.IsTrue(response.Succeeded, response.ProblemDetails);
        Assert.AreEqual(0, handler.CreateRequests);
        Assert.AreEqual(1, handler.UpdateRequests);
        Assert.AreEqual("updated", response.ResponseObject.Single().Foo);
    }

    [TestMethod]
    public async Task Patch_that_creates_and_updates_checks_both_authorizations()
    {
        // Arrange
        var (service, handler, _) = CreateSut();
        var patch = CreatePatch()
            .Add("/-", new TestDto { Foo = "created" })
            .Replace($"/{ExistingId}/foo", "updated");

        // Act
        var response = await service.PatchAsync(patch, CancellationToken.None);

        // Assert
        Assert.IsTrue(response.Succeeded, response.ProblemDetails);
        Assert.AreEqual(1, handler.CreateRequests);
        Assert.AreEqual(1, handler.UpdateRequests);
        Assert.AreEqual(2, response.ResponseObject.Count);
        CollectionAssert.AreEquivalent(new[] { "created", "updated" }, response.ResponseObject.Select(d => d.Foo).ToArray());
    }

    [TestMethod]
    public async Task Patch_that_only_updates_is_not_allowed_without_update_permission_even_if_creating_is_allowed()
    {
        // Arrange
        var (service, handler, factory) = CreateSut(updateStatus: HttpStatusCode.Forbidden);
        var patch = CreatePatch().Replace($"/{ExistingId}/foo", "updated");

        // Act
        var response = await service.PatchAsync(patch, CancellationToken.None);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.Status);
        Assert.AreEqual(0, handler.CreateRequests);
        Assert.AreEqual("original", await GetFooAsync(factory, ExistingId));
    }

    [TestMethod]
    public async Task Patch_that_only_creates_is_not_allowed_without_create_permission_even_if_updating_is_allowed()
    {
        // Arrange
        var (service, handler, factory) = CreateSut(createStatus: HttpStatusCode.Forbidden);
        var patch = CreatePatch().Add("/-", new TestDto { Foo = "created" });

        // Act
        var response = await service.PatchAsync(patch, CancellationToken.None);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.Status);
        Assert.AreEqual(0, handler.UpdateRequests);
        Assert.AreEqual(1, await CountAsync(factory));
    }

    [TestMethod]
    public async Task Patch_that_creates_and_updates_creates_nothing_if_update_is_denied()
    {
        // Arrange
        var (service, handler, factory) = CreateSut(updateStatus: HttpStatusCode.Forbidden);
        var patch = CreatePatch()
            .Add("/-", new TestDto { Foo = "created" })
            .Replace($"/{ExistingId}/foo", "updated");

        // Act
        var response = await service.PatchAsync(patch, CancellationToken.None);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.Status);
        Assert.AreEqual(0, handler.CreateRequests);
        Assert.AreEqual(1, await CountAsync(factory));
        Assert.AreEqual("original", await GetFooAsync(factory, ExistingId));
    }

    [TestMethod]
    public async Task Patch_that_creates_and_updates_stores_nothing_if_create_is_denied()
    {
        // Arrange
        // The whole patch is atomic: even though the update is authorized first, it must not be stored when the create is denied.
        var (service, handler, factory) = CreateSut(createStatus: HttpStatusCode.Forbidden);
        var patch = CreatePatch()
            .Add("/-", new TestDto { Foo = "created" })
            .Replace($"/{ExistingId}/foo", "updated");

        // Act
        var response = await service.PatchAsync(patch, CancellationToken.None);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.Status);
        Assert.AreEqual(1, handler.CreateRequests);
        Assert.AreEqual(1, handler.UpdateRequests);
        Assert.AreEqual(1, await CountAsync(factory));
        Assert.AreEqual("original", await GetFooAsync(factory, ExistingId));
    }

    [TestMethod]
    public async Task Patch_with_invalid_create_value_is_rejected_without_checking_create_authorization()
    {
        // Arrange
        var (service, handler, factory) = CreateSut();
        var patch = CreatePatch()
            .Replace($"/{ExistingId}/foo", "updated")
            .Add("/-", "not an object");

        // Act
        var response = await service.PatchAsync(patch, CancellationToken.None);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(0, handler.CreateRequests);
        Assert.AreEqual(1, handler.UpdateRequests);
        Assert.AreEqual("original", await GetFooAsync(factory, ExistingId));
    }

    [TestMethod]
    public void Typed_operations_prefix_the_path_with_the_id_and_use_the_json_property_name()
    {
        // Arrange
        var patch = new JsonPatchDocument<TestDto, TestDto> { SerializerOptions = JsonSerializerOptions.Web };

        // Act
        patch
            .Replace(5, d => d.Foo, "updated")
            .Test(5, d => d.Timestamp, new byte[] { 1 })
            .Remove(5, d => d.Foo)
            .Copy(5, d => d.Foo, d => d.Foo)
            .Create(new TestDto { Foo = "created" });

        // Assert
        CollectionAssert.AreEqual(
            new[] { "/5/foo", "/5/timestamp", "/5/foo", "/5/foo", "/-" },
            patch.Operations.Select(o => o.path).ToArray());
        Assert.AreEqual("/5/foo", patch.Operations[3].from);
    }

    [TestMethod]
    public async Task Typed_operations_can_be_used_to_patch()
    {
        // Arrange
        var (service, _, factory) = CreateSut();
        var patch = new JsonPatchDocument<TestDto, TestDto> { SerializerOptions = JsonSerializerOptions.Web }
            .Replace(ExistingId, d => d.Foo, "updated")
            .Create(new TestDto { Foo = "created" });

        // Act
        var response = await service.PatchAsync(patch, CancellationToken.None);

        // Assert
        Assert.IsTrue(response.Succeeded, response.ProblemDetails);
        Assert.AreEqual("updated", await GetFooAsync(factory, ExistingId));
        Assert.AreEqual(2, await CountAsync(factory));
    }

    [TestMethod]
    public void Patch_document_survives_a_json_round_trip()
    {
        // Arrange
        var patch = new JsonPatchDocument<TestDto, TestDto> { SerializerOptions = JsonSerializerOptions.Web }
            .Replace(5, d => d.Foo, "updated")
            .Move("/5/foo", "/5/bar")
            .Create(new TestDto { Foo = "created" });

        // Act
        var json = JsonSerializer.Serialize(patch, JsonSerializerOptions.Web);
        var roundTripped = JsonSerializer.Deserialize<JsonPatchDocument<TestDto, TestDto>>(json, JsonSerializerOptions.Web);

        // Assert
        Assert.IsNotNull(roundTripped);
        Assert.StartsWith("[", json);
        CollectionAssert.AreEqual(
            patch.Operations.Select(o => (o.op, o.path, o.from)).ToArray(),
            roundTripped.Operations.Select(o => (o.op, o.path, o.from)).ToArray());

        // "from" is only written for move and copy, as RFC 6902 defines.
        using var document = JsonDocument.Parse(json);
        var elements = document.RootElement.EnumerateArray().ToArray();
        Assert.IsFalse(elements[0].TryGetProperty("from", out _));
        Assert.AreEqual("/5/foo", elements[1].GetProperty("from").GetString());
        Assert.IsFalse(elements[2].TryGetProperty("from", out _));
    }

    [TestMethod]
    public void Patch_document_is_read_from_json_with_both_dto_types()
    {
        // Arrange
        const string json = """[{ "op": "add", "path": "/-", "value": { "foo": "created" } }, { "op": "replace", "path": "/1/foo", "value": "updated" }]""";

        // Act
        var patch = JsonSerializer.Deserialize<JsonPatchDocument<TestDto, TestDto>>(json, JsonSerializerOptions.Web);

        // Assert
        Assert.IsNotNull(patch);
        var split = patch.Split();
        Assert.AreEqual(1, split.Creates.Operations.Count);
        Assert.AreEqual(1, split.Updates[1].Operations.Count);
        Assert.AreEqual("/foo", split.Updates[1].Operations.Single().path);
        Assert.AreEqual(0, split.Invalid.Count);
    }

    [TestMethod]
    public async Task Patch_for_unknown_id_returns_not_found_and_does_not_create()
    {
        // Arrange
        var (service, _, factory) = CreateSut();
        var patch = CreatePatch()
            .Add("/-", new TestDto { Foo = "created" })
            .Replace("/999/foo", "updated");

        // Act
        var response = await service.PatchAsync(patch, CancellationToken.None);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.Status);
        Assert.AreEqual(1, await CountAsync(factory));
    }

    [TestMethod]
    public async Task Patch_reports_all_problems_and_not_just_the_first_one()
    {
        // Arrange
        var (service, handler, factory) = CreateSut();
        var patch = CreatePatch()
            .Replace("/998/foo", "updated")
            .Replace("/999/foo", "updated")
            .Replace($"/{ExistingId}/id", 2L);

        // Act
        var response = await service.PatchAsync(patch, CancellationToken.None);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.Status);
        Assert.IsNotNull(response.ValidationResults);
        CollectionAssert.AreEquivalent(new[] { "/998", "/999", $"/{ExistingId}/id" }, response.ValidationResults.Keys.ToArray());
        Assert.AreEqual(0, handler.CreateRequests);
        Assert.AreEqual(1, handler.UpdateRequests);
        Assert.AreEqual("original", await GetFooAsync(factory, ExistingId));
    }

    [TestMethod]
    [DataRow("/foo")]
    [DataRow("/abc/foo")]
    [DataRow("/1")]
    public async Task Patch_with_invalid_path_is_rejected_before_any_authorization(string path)
    {
        // Arrange
        var (service, handler, factory) = CreateSut();
        var patch = CreatePatch().Replace(path, "updated");

        // Act
        var response = await service.PatchAsync(patch, CancellationToken.None);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(0, handler.CreateRequests);
        Assert.AreEqual(0, handler.UpdateRequests);
        Assert.AreEqual("original", await GetFooAsync(factory, ExistingId));
    }

    [TestMethod]
    public async Task Patch_cannot_change_the_id()
    {
        // Arrange
        var (service, handler, factory) = CreateSut();
        var patch = CreatePatch().Replace($"/{ExistingId}/id", 2L);

        // Act
        var response = await service.PatchAsync(patch, CancellationToken.None);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(1, handler.UpdateRequests);
        Assert.AreEqual("original", await GetFooAsync(factory, ExistingId));
    }

    [TestMethod]
    public async Task Patch_with_failing_test_operation_writes_nothing()
    {
        // Arrange
        var (service, handler, factory) = CreateSut();
        var patch = CreatePatch()
            .Test($"/{ExistingId}/foo", "something else")
            .Replace($"/{ExistingId}/foo", "updated");

        // Act
        var response = await service.PatchAsync(patch, CancellationToken.None);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.Status);
        Assert.AreEqual(1, handler.UpdateRequests);
        Assert.AreEqual("original", await GetFooAsync(factory, ExistingId));
    }

    private static JsonPatchDocument<TestDto, TestDto> CreatePatch() => new() { SerializerOptions = JsonSerializerOptions.Web };

    private static async Task<string?> GetFooAsync(IDbContextFactory<TestDatabase> factory, long id)
    {
        await using var context = factory.CreateDbContext();
        return (await context.TestModels.SingleAsync(m => m.Id == id)).Foo;
    }

    private static async Task<int> CountAsync(IDbContextFactory<TestDatabase> factory)
    {
        await using var context = factory.CreateDbContext();
        return await context.TestModels.CountAsync();
    }

    private static (ICrudServiceBase<TestModel, TestDto, TestDto, TestDto, TestDto, TestDto> Service, RecordingAuthorizationHandler Handler, IDbContextFactory<TestDatabase> Factory) CreateSut(
        HttpStatusCode createStatus = HttpStatusCode.OK,
        HttpStatusCode updateStatus = HttpStatusCode.OK)
    {
        var handler = new RecordingAuthorizationHandler(createStatus, updateStatus);

        var environment = new TestBuilder()
            .WithInMemoryDatabase<TestDatabase>()
            .WithSeed(new TestModel { Id = ExistingId, Foo = "original" })
            .WithAutomapper(cfg =>
            {
                cfg.CreateMap<TestModel, TestDto>();
                cfg.CreateMap<TestDto, TestModel>();
            })
            .WithoutUser()
            .WithValidation()
            .WithCrudAutoMapper<TestModel, TestDto, TestDto, TestDto, TestDto, TestDto>()
            .WithService<ICrudAuthorizationHandler<TestModel, TestDto, TestDto, TestDto, TestDto, TestDto>, RecordingAuthorizationHandler>(handler)
            .WithCrudService<TestDatabase, TestModel, TestDto>()
            .BuildWithServiceAsSut();

        return (environment.GetSut(), handler, environment.GetRequiredService<IDbContextFactory<TestDatabase>>());
    }
}

/// <summary>
/// Counts the request authorizations for create and update and answers with the configured status.
/// </summary>
internal sealed class RecordingAuthorizationHandler : CrudAuthorizationHandlerBase<TestModel, TestDto, TestDto, TestDto, TestDto, TestDto>
{
    private readonly HttpStatusCode _createStatus;
    private readonly HttpStatusCode _updateStatus;

    public RecordingAuthorizationHandler(HttpStatusCode createStatus, HttpStatusCode updateStatus)
    {
        _createStatus = createStatus;
        _updateStatus = updateStatus;
    }

    public int CreateRequests { get; private set; }

    public int UpdateRequests { get; private set; }

    public override Task<AuthorizationResult<TestModel, TestDto>> HandleCreateRequestAsync(AuthorizationResult<TestModel, TestDto> previousResult, CancellationToken cancellationToken)
    {
        CreateRequests++;
        return Task.FromResult(previousResult.WithStatus(_createStatus));
    }

    public override Task<AuthorizationResult<TestModel, IReadOnlyCollection<TestDto>>> HandleCreateRequestAsync(AuthorizationResult<TestModel, IReadOnlyCollection<TestDto>> previousResult, CancellationToken cancellationToken)
    {
        CreateRequests++;
        return Task.FromResult(previousResult.WithStatus(_createStatus));
    }

    public override Task<AuthorizationResult<TestModel, TestDto>> HandleUpdateRequestAsync(AuthorizationResult<TestModel, TestDto> previousResult, CancellationToken cancellationToken)
    {
        UpdateRequests++;
        return Task.FromResult(previousResult.WithStatus(_updateStatus));
    }

    public override Task<AuthorizationResult<TestModel, IUpdateMultipleRequest<TestDto, TestModel>>> HandleUpdateRequestAsync(AuthorizationResult<TestModel, IUpdateMultipleRequest<TestDto, TestModel>> previousResult, CancellationToken cancellationToken)
    {
        UpdateRequests++;
        return Task.FromResult(previousResult.WithStatus(_updateStatus));
    }
}
