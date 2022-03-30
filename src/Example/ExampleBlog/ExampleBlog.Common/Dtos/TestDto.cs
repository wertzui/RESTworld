using HAL.Common.Converters;
using RESTworld.Common.Dtos;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExampleBlog.Common.Dtos
{
    public class TestDto : ConcurrentDtoBase
    {
        public string MyString { get; set; } = default!;
        public int MyInt { get; set; }
        public char MyChar { get; set; }
        public bool MyBool { get; set; }
        public string? MyNullableString { get; set; }
        public int? MyNullableInt { get; set; }
        public char? MyNullableChar { get; set; }
        public bool? MyNullableBool { get; set; }
        public decimal? MyNullableDecimal { get; set; }
        public ICollection<ListTestDto> MyCollection { get; set; } = new HashSet<ListTestDto>();
        public ICollection<ListChangeTrackingDto>? MyNullCollection { get; set; }
        [JsonConverter(typeof(KeyValueDictionaryConverterFactory))]
        public IDictionary<string, DictionaryTestDto> MyDictionary { get; set; } = new Dictionary<string, DictionaryTestDto>();
        public NestedTestDto? MyNestedObject { get; set; }
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
}
