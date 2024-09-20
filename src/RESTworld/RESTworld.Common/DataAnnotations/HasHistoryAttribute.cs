namespace System.ComponentModel.DataAnnotations;

/// <summary>
/// This attribute is used to indicate that the entity has a history.
/// It is read by various RESTworld components to determine if the entity has a history.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class HasHistoryAttribute : Attribute
{
}