using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson.Operations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RESTworld.Business.Models;

/// <summary>
/// A JSON Patch (RFC 6902) document which targets a whole collection and can both create and update entities.
/// It is modelled after <see cref="JsonPatchDocument{TModel}"/>, but that type has only one model for all operations,
/// while creating and updating use different DTOs.
/// <list type="bullet">
///     <item><c>{ "op": "add", "path": "/-", "value": { ... } }</c> creates a new entity. The value is a <typeparamref name="TCreate"/>.</item>
///     <item>Every other operation uses the path <c>/{id}/{property}</c> and is applied to the <typeparamref name="TUpdate"/> of the entity with that ID.</item>
/// </list>
/// Like <see cref="JsonPatchDocument{TModel}"/> this is only typed in C#. On the wire it is a normal JSON Patch array, as RFC 6902 requires.
/// Call <see cref="Split"/> to get a regular <see cref="JsonPatchDocument{TModel}"/> for the creates and one per updated entity, which can then be applied with the standard <c>ApplyTo</c> methods.
/// </summary>
/// <typeparam name="TCreate">The type of the DTO which is used to create entities.</typeparam>
/// <typeparam name="TUpdate">The type of the DTO which is used to update entities.</typeparam>
[JsonConverter(typeof(JsonPatchDocumentOfCreateAndUpdateConverterFactory))]
public class JsonPatchDocument<TCreate, TUpdate> : IJsonPatchDocument
    where TUpdate : class
{
    /// <summary>
    /// The path of an <c>add</c> operation which creates a new entity.
    /// </summary>
    public const string CreatePath = "/-";

    /// <summary>
    /// Creates an empty document.
    /// </summary>
    public JsonPatchDocument()
    {
        Operations = [];
        SerializerOptions = JsonSerializerOptions.Default;
    }

    /// <summary>
    /// Creates a document from existing operations.
    /// </summary>
    /// <param name="operations">The operations.</param>
    /// <param name="serializerOptions">The options which are used to read the values of the operations and to resolve property names.</param>
    public JsonPatchDocument(List<Operation> operations, JsonSerializerOptions serializerOptions)
    {
        Operations = operations ?? throw new ArgumentNullException(nameof(operations));
        SerializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));
    }

    /// <summary>
    /// The operations in the order in which they appear in the JSON document.
    /// </summary>
    [JsonIgnore]
    public List<Operation> Operations { get; }

    /// <summary>
    /// The options which are used to read the values of the operations and to resolve property names.
    /// </summary>
    [JsonIgnore]
    public JsonSerializerOptions SerializerOptions { get; set; }

    /// <summary>
    /// Adds an operation which creates a new entity. Will result in, for example,
    /// { "op": "add", "path": "/-", "value": { "name": "foo" } }
    /// </summary>
    /// <param name="value">The entity to create.</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Create(TCreate value) => AddOperation("add", CreatePath, null, value);

    #region Operations with a raw path

    /// <summary>
    /// Add operation with a raw JSON Pointer. Use <see cref="Create"/> to create entities.
    /// </summary>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Add(string path, object? value) => AddOperation("add", path, null, value);

    /// <summary>
    /// Remove operation with a raw JSON Pointer.
    /// </summary>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Remove(string path) => AddOperation("remove", path, null, null);

    /// <summary>
    /// Replace operation with a raw JSON Pointer.
    /// </summary>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Replace(string path, object? value) => AddOperation("replace", path, null, value);

    /// <summary>
    /// Test operation with a raw JSON Pointer.
    /// </summary>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Test(string path, object? value) => AddOperation("test", path, null, value);

    /// <summary>
    /// Move operation with raw JSON Pointers. Both must point into the same entity.
    /// </summary>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Move(string from, string path) => AddOperation("move", path, from, null);

    /// <summary>
    /// Copy operation with raw JSON Pointers. Both must point into the same entity.
    /// </summary>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Copy(string from, string path) => AddOperation("copy", path, from, null);

    #endregion

    #region Typed operations on the update DTO of the entity with the given ID

    /// <summary>
    /// Add operation.  Will result in, for example,
    /// { "op": "add", "path": "/5/a/b/c", "value": [ "foo", "bar" ] }
    /// </summary>
    /// <param name="id">The ID of the entity to update.</param>
    /// <param name="path">target location</param>
    /// <param name="value">value</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Add<TProp>(long id, Expression<Func<TUpdate, TProp>> path, TProp value)
        => AddOperation("add", GetPath(id, path, null), null, value);

    /// <summary>
    /// Add value to list at given position
    /// </summary>
    /// <param name="id">The ID of the entity to update.</param>
    /// <param name="path">target location</param>
    /// <param name="value">value</param>
    /// <param name="position">position</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Add<TProp>(long id, Expression<Func<TUpdate, IList<TProp>>> path, TProp value, int position)
        => AddOperation("add", GetPath(id, path, position.ToString(CultureInfo.InvariantCulture)), null, value);

    /// <summary>
    /// Add value to the end of the list
    /// </summary>
    /// <param name="id">The ID of the entity to update.</param>
    /// <param name="path">target location</param>
    /// <param name="value">value</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Add<TProp>(long id, Expression<Func<TUpdate, IList<TProp>>> path, TProp value)
        => AddOperation("add", GetPath(id, path, "-"), null, value);

    /// <summary>
    /// Remove value at target location.  Will result in, for example,
    /// { "op": "remove", "path": "/5/a/b/c" }
    /// </summary>
    /// <param name="id">The ID of the entity to update.</param>
    /// <param name="path">target location</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Remove<TProp>(long id, Expression<Func<TUpdate, TProp>> path)
        => AddOperation("remove", GetPath(id, path, null), null, null);

    /// <summary>
    /// Remove value from list at given position
    /// </summary>
    /// <param name="id">The ID of the entity to update.</param>
    /// <param name="path">target location</param>
    /// <param name="position">position</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Remove<TProp>(long id, Expression<Func<TUpdate, IList<TProp>>> path, int position)
        => AddOperation("remove", GetPath(id, path, position.ToString(CultureInfo.InvariantCulture)), null, null);

    /// <summary>
    /// Remove value from end of list
    /// </summary>
    /// <param name="id">The ID of the entity to update.</param>
    /// <param name="path">target location</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Remove<TProp>(long id, Expression<Func<TUpdate, IList<TProp>>> path)
        => AddOperation("remove", GetPath(id, path, "-"), null, null);

    /// <summary>
    /// Replace value.  Will result in, for example,
    /// { "op": "replace", "path": "/5/a/b/c", "value": 42 }
    /// </summary>
    /// <param name="id">The ID of the entity to update.</param>
    /// <param name="path">target location</param>
    /// <param name="value">value</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Replace<TProp>(long id, Expression<Func<TUpdate, TProp>> path, TProp value)
        => AddOperation("replace", GetPath(id, path, null), null, value);

    /// <summary>
    /// Replace value in a list at given position
    /// </summary>
    /// <param name="id">The ID of the entity to update.</param>
    /// <param name="path">target location</param>
    /// <param name="value">value</param>
    /// <param name="position">position</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Replace<TProp>(long id, Expression<Func<TUpdate, IList<TProp>>> path, TProp value, int position)
        => AddOperation("replace", GetPath(id, path, position.ToString(CultureInfo.InvariantCulture)), null, value);

    /// <summary>
    /// Replace value at end of a list
    /// </summary>
    /// <param name="id">The ID of the entity to update.</param>
    /// <param name="path">target location</param>
    /// <param name="value">value</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Replace<TProp>(long id, Expression<Func<TUpdate, IList<TProp>>> path, TProp value)
        => AddOperation("replace", GetPath(id, path, "-"), null, value);

    /// <summary>
    /// Test value.  Will result in, for example,
    /// { "op": "test", "path": "/5/a/b/c", "value": 42 }
    /// </summary>
    /// <param name="id">The ID of the entity to update.</param>
    /// <param name="path">target location</param>
    /// <param name="value">value</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Test<TProp>(long id, Expression<Func<TUpdate, TProp>> path, TProp value)
        => AddOperation("test", GetPath(id, path, null), null, value);

    /// <summary>
    /// Test value in a list at given position
    /// </summary>
    /// <param name="id">The ID of the entity to update.</param>
    /// <param name="path">target location</param>
    /// <param name="value">value</param>
    /// <param name="position">position</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Test<TProp>(long id, Expression<Func<TUpdate, IList<TProp>>> path, TProp value, int position)
        => AddOperation("test", GetPath(id, path, position.ToString(CultureInfo.InvariantCulture)), null, value);

    /// <summary>
    /// Test value at end of a list
    /// </summary>
    /// <param name="id">The ID of the entity to update.</param>
    /// <param name="path">target location</param>
    /// <param name="value">value</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Test<TProp>(long id, Expression<Func<TUpdate, IList<TProp>>> path, TProp value)
        => AddOperation("test", GetPath(id, path, "-"), null, value);

    /// <summary>
    /// Removes value at specified location and add it to the target location.  Will result in, for example:
    /// { "op": "move", "from": "/5/a/b/c", "path": "/5/a/b/d" }
    /// </summary>
    /// <param name="id">The ID of the entity to update. Moving between entities is not possible.</param>
    /// <param name="from">source location</param>
    /// <param name="path">target location</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Move<TProp>(long id, Expression<Func<TUpdate, TProp>> from, Expression<Func<TUpdate, TProp>> path)
        => AddOperation("move", GetPath(id, path, null), GetPath(id, from, null), null);

    /// <summary>
    /// Copy the value at specified location to the target location.  Will result in, for example:
    /// { "op": "copy", "from": "/5/a/b/c", "path": "/5/a/b/e" }
    /// </summary>
    /// <param name="id">The ID of the entity to update. Copying between entities is not possible.</param>
    /// <param name="from">source location</param>
    /// <param name="path">target location</param>
    /// <returns>The <see cref="JsonPatchDocument{TCreate, TUpdate}"/> for chaining.</returns>
    public JsonPatchDocument<TCreate, TUpdate> Copy<TProp>(long id, Expression<Func<TUpdate, TProp>> from, Expression<Func<TUpdate, TProp>> path)
        => AddOperation("copy", GetPath(id, path, null), GetPath(id, from, null), null);

    #endregion

    /// <summary>
    /// Splits the operations into the ones which create entities, the ones which update a single entity and the ones which are neither.
    /// The result reflects the current <see cref="Operations"/>, so call it again after changing them.
    /// </summary>
    public JsonPatchSplit<TCreate, TUpdate> Split()
    {
        var creates = new List<Operation<List<TCreate>>>();
        var updates = new Dictionary<long, List<Operation<TUpdate>>>();
        var invalid = new List<Operation>();

        foreach (var operation in Operations)
        {
            if (operation.OperationType == OperationType.Add && operation.path == CreatePath)
            {
                creates.Add(new Operation<List<TCreate>>(operation.op, operation.path, operation.from, operation.value));
                continue;
            }

            if (!TrySplitPath(operation.path, out var id, out var relativePath))
            {
                invalid.Add(operation);
                continue;
            }

            // move and copy must stay within the same entity.
            string? relativeFrom = null;
            if (operation.from is not null && (!TrySplitPath(operation.from, out var fromId, out relativeFrom) || fromId != id))
            {
                invalid.Add(operation);
                continue;
            }

            if (!updates.TryGetValue(id, out var operations))
                updates[id] = operations = [];

            operations.Add(new Operation<TUpdate>(operation.op, relativePath, relativeFrom!, operation.value));
        }

        return new JsonPatchSplit<TCreate, TUpdate>(
            new JsonPatchDocument<List<TCreate>>(creates, SerializerOptions),
            updates.ToDictionary(pair => pair.Key, pair => new JsonPatchDocument<TUpdate>(pair.Value, SerializerOptions)),
            invalid);
    }

    IList<Operation> IJsonPatchDocument.GetOperations() => Operations;

    private JsonPatchDocument<TCreate, TUpdate> AddOperation(string op, string path, string? from, object? value)
    {
        ArgumentNullException.ThrowIfNull(path);

        Operations.Add(new Operation(op, path, from!, value!));

        return this;
    }

    private static bool TrySplitPath(string? path, out long id, out string relativePath)
    {
        id = 0;
        relativePath = "";

        if (path is null || path.Length < 2 || path[0] != '/')
            return false;

        var end = path.IndexOf('/', 1);

        // Operations on the entity as a whole (/5) are not supported. Deleting has its own endpoint.
        if (end < 0 || end == path.Length - 1)
            return false;

        relativePath = path[end..];
        return long.TryParse(path.AsSpan(1, end - 1), NumberStyles.None, CultureInfo.InvariantCulture, out id);
    }

    // Same as in JsonPatchDocument<TModel>, but prefixed with the ID of the entity.
    private string GetPath<TProp>(long id, Expression<Func<TUpdate, TProp>> expr, string? position)
    {
        ArgumentNullException.ThrowIfNull(expr);

        var path = "/" + id.ToString(CultureInfo.InvariantCulture);

        var segments = GetPathSegments(expr.Body);
        if (segments.Count > 0)
            path += "/" + string.Join('/', segments);

        if (position is not null)
            path += "/" + position;

        return path;
    }

    private List<string> GetPathSegments(Expression expr)
    {
        var segments = new List<string>();
        switch (expr.NodeType)
        {
            case ExpressionType.ArrayIndex:
                var binary = (BinaryExpression)expr;
                segments.AddRange(GetPathSegments(binary.Left));
                segments.Add(binary.Right.ToString());
                return segments;

            case ExpressionType.Call:
                var call = (MethodCallExpression)expr;
                if (call.Object is null || call.Method.Name != "get_Item")
                    goto default;
                segments.AddRange(GetPathSegments(call.Object));
                segments.Add(EvaluateExpression(call.Arguments[0]));
                return segments;

            case ExpressionType.Convert:
                segments.AddRange(GetPathSegments(((UnaryExpression)expr).Operand));
                return segments;

            case ExpressionType.MemberAccess:
                var member = (MemberExpression)expr;
                segments.AddRange(GetPathSegments(member.Expression!));
                segments.Add(GetPropertyName(member));
                return segments;

            case ExpressionType.Parameter:
                // Fits "x => x" (the whole entity).
                return segments;

            default:
                throw new InvalidOperationException($"The expression '{expr}' is not supported. Only property access, indexers and conversions can be used to describe the path.");
        }
    }

    // Respects the naming policy and attributes like [JsonPropertyName].
    private string GetPropertyName(MemberExpression member)
    {
        var typeInfo = SerializerOptions.GetTypeInfo(member.Expression!.Type);
        var property = typeInfo.Properties.FirstOrDefault(p => p.AttributeProvider is MemberInfo info && info == member.Member);

        return property?.Name ?? member.Member.Name;
    }

    // Evaluates the key or index of an indexer, which may be an int, a string or some other expression.
    private static string EvaluateExpression(Expression expression)
    {
        var lambda = Expression.Lambda<Func<object, object>>(Expression.Convert(expression, typeof(object)), Expression.Parameter(typeof(object), null));

        return Convert.ToString(lambda.Compile()(null!), CultureInfo.InvariantCulture)!;
    }
}

/// <summary>
/// The result of <see cref="JsonPatchDocument{TCreate, TUpdate}.Split"/>.
/// </summary>
/// <param name="Creates">
/// A regular patch document for a list. Apply it to an empty <see cref="List{T}"/> of <typeparamref name="TCreate"/> to get the entities to create.
/// </param>
/// <param name="Updates">
/// A regular patch document per entity ID. The paths are relative to the entity, so <c>/5/title</c> is <c>/title</c> in the document for the ID <c>5</c>.
/// Apply it to the current <typeparamref name="TUpdate"/> of that entity.
/// </param>
/// <param name="Invalid">Operations which have a path that is neither a create nor an update of a single entity.</param>
/// <typeparam name="TCreate">The type of the DTO which is used to create entities.</typeparam>
/// <typeparam name="TUpdate">The type of the DTO which is used to update entities.</typeparam>
public sealed record JsonPatchSplit<TCreate, TUpdate>(
    JsonPatchDocument<List<TCreate>> Creates,
    IReadOnlyDictionary<long, JsonPatchDocument<TUpdate>> Updates,
    IReadOnlyList<Operation> Invalid)
    where TUpdate : class;

/// <summary>
/// Reads and writes <see cref="JsonPatchDocument{TCreate, TUpdate}"/> as a normal JSON Patch array.
/// The converter of the package is internal, so this does what it does: it reads the operations and keeps the serializer options.
/// </summary>
internal sealed class JsonPatchDocumentOfCreateAndUpdateConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(JsonPatchDocument<,>);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => (JsonConverter)Activator.CreateInstance(typeof(Converter<,>).MakeGenericType(typeToConvert.GetGenericArguments()))!;

    private sealed class Converter<TCreate, TUpdate> : JsonConverter<JsonPatchDocument<TCreate, TUpdate>>
        where TUpdate : class
    {
        public override JsonPatchDocument<TCreate, TUpdate>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var operations = JsonSerializer.Deserialize<List<Operation>>(ref reader, options);

            return operations is null ? null : new JsonPatchDocument<TCreate, TUpdate>(operations, options);
        }

        public override void Write(Utf8JsonWriter writer, JsonPatchDocument<TCreate, TUpdate> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var operation in value.Operations)
            {
                writer.WriteStartObject();
                writer.WriteString("op", operation.op);
                writer.WriteString("path", operation.path);

                if (operation.ShouldSerializeFrom())
                    writer.WriteString("from", operation.from);

                if (operation.OperationType is OperationType.Add or OperationType.Replace or OperationType.Test)
                {
                    writer.WritePropertyName("value");
                    JsonSerializer.Serialize(writer, operation.value, options);
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }
    }
}
