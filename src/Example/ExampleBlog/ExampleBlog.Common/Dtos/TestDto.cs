using HAL.Common.Converters;
using RESTworld.Common.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ExampleBlog.Common.Dtos;

public class TestDto : ConcurrentDtoBase
{
    [ForeignKey(nameof(Blogs))]
    [DisplayName("Blogs")]
    public required IReadOnlyCollection<long> BlogIds { get; set; }
    [Display(Name = "Post")]
    public required long PostId { get; set; }
    [Required]
    public string? MyRequiredString { get; set; }
    public string MyString { get; set; } = default!;
    public int MyInt { get; set; }
    public char MyChar { get; set; }
    public bool MyBool { get; set; }
    public TestEnum MyEnum { get; set; }
    [Editable(false)]
    public TestEnum MyReadonlyEnum { get; set; }
    public TestFlagsEnum MyFlagsEnum { get; set; }
    public string? MyNullableString { get; set; }
    public int? MyNullableInt { get; set; }
    public char? MyNullableChar { get; set; }
    public bool? MyNullableBool { get; set; }
    public TestEnum? MyNullableEnum { get; set; }
    public decimal? MyNullableDecimal { get; set; }
    public DateTimeOffset MyDateTimeOffset { get; set; }
    public DateOnly MyDateOnly { get; set; }
    public TimeOnly MyTimeOnly { get; set; }
    public ICollection<ListTestDto> MyCollection { get; set; } = new HashSet<ListTestDto>();
    public ICollection<ListChangeTrackingDto>? MyNullCollection { get; set; }
    [JsonConverter(typeof(KeyValueDictionaryConverterFactory))]
    public IDictionary<string, DictionaryTestDto> MyDictionary { get; set; } = new Dictionary<string, DictionaryTestDto>();
    public NestedTestDto? MyNestedObject { get; set; }

    [JsonIgnore]
    public virtual ICollection<BlogDto>? Blogs { get; }

    [JsonIgnore]
    public virtual PostListDto? Post { get; }
}

public class NestedTestDto
{
    public string MyNestedString { get; set; } = default!;
    public int MyNestedInt { get; set; }
}

public class ListTestDto
{
    public string MyListString { get; set; } = default!;
    public int MyListInt { get; set; }
}

public class DictionaryTestDto
{
    public string MyDictionaryString { get; set; } = default!;
    public int MyDictionaryInt { get; set; }
}

public class ListChangeTrackingDto : ChangeTrackingDtoBase
{
    public string MyListString { get; set; } = default!;
    public int MyListInt { get; set; }
}

public enum TestEnum
{
    TestA,
    TestB,
    TestC
}

[Flags]
public enum TestFlagsEnum
{
    TestFlagA = 1,
    TestFlagB = 2,
    TestFlagC = 4
}
