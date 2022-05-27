using AutoFixture;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Services;
using RESTworld.Common.Dtos;
using RESTworld.Testing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RESTworld.Tests.Business
{
    [TestClass]
    public class TestBuilderTests
    {
        [TestMethod]
        public void WithUser_should_add_the_given_user()
        {
            // Arrange
            var fixture = new Fixture();
            var expectedUsername = fixture.Create<string>();
            var expectedAuthenticationType = fixture.Create<string>();

            // Act
            var environment = new TestBuilder()
                .WithUser(expectedUsername, expectedAuthenticationType)
                .Build<MyService>();

            // Assert
            var userAccessor = environment.GetService<IUserAccessor>();
            Assert.IsNotNull(userAccessor);
            var user = userAccessor.User;
            Assert.IsNotNull(user);
            var identity = user.Identity;
            Assert.IsNotNull(identity);
            Assert.AreEqual(expectedUsername, identity.Name);
            Assert.AreEqual(expectedAuthenticationType, identity.AuthenticationType);
        }

        [TestMethod]
        public void WithAutomapper_should_add_Automapper_using_the_given_configuration()
        {
            // Arrange
            var fixture = new Fixture();
            var model = fixture.Create<TestModel>();

            // Act
            var environment = new TestBuilder()
                .WithAutomapper(cfg => cfg.CreateMap<TestModel, TestDto>())
                .Build<MyService>();

            // Assert
            var mapper = environment.GetService<IMapper>();
            Assert.IsNotNull(mapper);
            var dto = mapper.Map<TestDto>(model);
            Assert.IsNotNull(dto);
            Assert.AreEqual(model.Id, dto.Id);
            Assert.AreEqual(model.Foo, dto.Foo);
            CollectionAssert.AreEqual(model.Timestamp, dto.Timestamp);
        }

        [TestMethod]
        public void WithInMemoryDatabase_should_add_the_configred_IDbContextFactory()
        {
            // Arrange

            // Act
            var environment = new TestBuilder()
                .WithInMemoryDatabase<TestDatabase>()
                .Build<MyService>();

            // Assert
            var actualFactory = environment.GetService<IDbContextFactory<TestDatabase>>();
            Assert.IsNotNull(actualFactory);
            using var actualContext = actualFactory.CreateDbContext();
            Assert.IsNotNull(actualContext);
        }

        [TestMethod]
        public void WithInMemoryDatabase_should_add_the_configred_DbContext()
        {
            // Arrange

            // Act
            var environment = new TestBuilder()
                .WithInMemoryDatabase<TestDatabase>()
                .Build<MyService>();

            // Assert
            using var actualContext = environment.GetService<TestDatabase>();
            Assert.IsNotNull(actualContext);
        }

        [TestMethod]
        public async Task WithSeedData_should_add_the_given_data()
        {
            // Arrange
            var fixture = new Fixture();
            var expectedModel = fixture.Create<TestModel>();

            // Act
            var environment = new TestBuilder()
                .WithInMemoryDatabase<TestDatabase>()
                .WithSeed(expectedModel)
                .Build<MyService>();

            // Assert
            var actualFactory = environment.GetService<IDbContextFactory<TestDatabase>>();
            Assert.IsNotNull(actualFactory);
            var actualModel = await actualFactory.Parallel().Set<TestModel>().SingleAsync();
            Assert.AreEqual(expectedModel.Id, actualModel.Id);
            Assert.AreEqual(expectedModel.Foo, actualModel.Foo);
        }

        [TestMethod]
        public void Sut_should_be_created_with_all_dependencies()
        {
            // Arrange
            var environment = new TestBuilder()
                .WithInMemoryDatabase<TestDatabase>()
                .WithAutomapper(cfg => { })
                .WithoutUser()
                .Build<MyService>();

            // Act
            var sut = environment.GetSut();

            // Assert
            Assert.IsNotNull(sut);
        }

        [TestMethod]
        public void ReadService_should_be_created_with_all_dependencies()
        {
            // Arrange
            var environment = new TestBuilder()
                .WithInMemoryDatabase<TestDatabase>()
                .WithAutomapper(cfg => { })
                .WithoutUser()
                .WithReadService<TestDatabase, TestModel, TestDto>()
                .BuildWithServiceAsSut();

            // Act
            var sut = environment.GetSut();

            // Assert
            Assert.IsNotNull(sut);
        }

        [TestMethod]
        public void CrudService_should_be_created_with_all_dependencies()
        {
            // Arrange
            var environment = new TestBuilder()
                .WithInMemoryDatabase<TestDatabase>()
                .WithAutomapper(cfg => { })
                .WithoutUser()
                .WithCrudService<TestDatabase, TestModel, TestDto>()
                .BuildWithServiceAsSut();

            // Act
            var sut = environment.GetSut();

            // Assert
            Assert.IsNotNull(sut);
        }

        [TestMethod]
        public void Options_can_be_created()
        {
            // Arrange
            var expectedValue = new Fixture().Create<string>();
            var environment = new TestBuilder()
                .WithOptions(expectedValue)
                .Build();

            // Act
            var options = environment.GetService<IOptions<string>>();

            // Assert
            Assert.IsNotNull(options);
            Assert.AreEqual(expectedValue, options.Value);
        }

        [TestMethod]
        public void Options_can_be_created_through_action()
        {
            // Arrange
            var expectedValue = new Fixture().Create<string>();
            var environment = new TestBuilder()
                .WithOptions<TestOptions>(o => o.Bar = expectedValue)
                .Build();

            // Act
            var options = environment.GetService<IOptions<TestOptions>>();

            // Assert
            Assert.IsNotNull(options);
            Assert.AreEqual(expectedValue, options.Value.Bar);
        }
    }

    public class TestDto : ConcurrentDtoBase
    {
        public string? Foo { get; set; }
    }

    public class TestOptions
    {
        public string? Bar { get; set; }
    }

    public class MyService : CrudServiceBase<TestDatabase, TestModel, TestDto, TestDto, TestDto, TestDto>
    {
        public MyService(IDbContextFactory<TestDatabase> contextFactory, IMapper mapper, IEnumerable<ICrudAuthorizationHandler<TestModel, TestDto, TestDto, TestDto, TestDto>> authorizationHandlers, IUserAccessor userAccessor, ILogger<CrudServiceBase<TestDatabase, TestModel, TestDto, TestDto, TestDto, TestDto>> logger) : base(contextFactory, mapper, authorizationHandlers, userAccessor, logger)
        {
        }
    }
}
