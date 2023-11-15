using HAL.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTworld.Common.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace RESTworld.Tests.Serialization;

[TestClass]
public class SerializationTests
{
    [TestMethod]
    public void HomeDto_can_be_deserialized()
    {
        // Arrange
        var expected = new Resource<HomeDto>
        {
            State = new HomeDto(new VersionInformationDto(new HashSet<string> { "1" }, new HashSet<string>()))
        };
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var json = JsonSerializer.Serialize(expected, options);

        // Act
        var actual = JsonSerializer.Deserialize<Resource<HomeDto>>(json, options);

        // Assert
        Assert.IsNotNull(actual);
        Assert.IsNotNull(actual.State);
        Assert.IsNotNull(actual.State.Versions);
        CollectionAssert.AreEquivalent(expected.State.Versions.Supported.ToList(), actual.State.Versions.Supported.ToList());
        CollectionAssert.AreEquivalent(expected.State.Versions.Deprecated.ToList(), actual.State.Versions.Deprecated.ToList());
    }
}
