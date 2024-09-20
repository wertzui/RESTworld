using ExampleBlog.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace ExampleBlog.Data.Models;

/// <summary>
/// This class and its relations which start with "Test" are not stored in the database, but are generated using AutoFixture which then simulate a cached list which might come from a database.
/// This is to test the functionality of using in memory collections as data source when querying.
/// </summary>
public class TestEntity : ConcurrentEntityBase
{
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
    public virtual Post? Post { get; }
}
