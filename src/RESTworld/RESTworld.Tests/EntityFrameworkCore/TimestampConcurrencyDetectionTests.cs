using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTworld.EntityFrameworkCore;
using RESTworld.EntityFrameworkCore.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace RESTworld.Tests
{
    [TestClass]
    public class TimestampConcurrencyDetectionTests
    {
        [TestMethod]
        public void SetOriginalTimestampValueForConcurrencyDetection_should_work_when_the_Timestamp_has_an_NotMapped_Attribute()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDatabase>().UseSqlServer().Options;
            using var context = new TestDatabase(options);
            var model = new TestModel { Id = 1 };
            context.TestModels.Attach(model);
            model.Foo = "changed"; // We changed a property. This will trigger the concurrency detection.

            // Act
            using (new TimestampConcurrencyDetection(context.ChangeTracker))
            {
                // The constructor will call the method that we want to test.
            }

            // Assert
            // If no exception is thrown, everything is fine.
        }
    }

    public class TestModel : ConcurrentEntityBase
    {
        [NotMapped]
        public override byte[]? Timestamp { get => base.Timestamp; set => base.Timestamp = value; }

        public string? Foo { get; set; }
    }

    public class TestDatabase : DbContextBase
    {
        public TestDatabase(DbContextOptions<TestDatabase> options) : base(options)
        {
        }

        public DbSet<TestModel> TestModels { get; set; } = default!;
    }
}