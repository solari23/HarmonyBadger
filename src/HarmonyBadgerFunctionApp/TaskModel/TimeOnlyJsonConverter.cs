using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HarmonyBadgerFunctionApp.TaskModel;

/// <summary>
/// A JSON converter that allows serializing and deserializing <see cref="TimeOnly"/> objects.
/// </summary>
public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    /// <summary>
    /// The default format for serializing <see cref="TimeOnly"/> values as strings.
    /// </summary>
    public const string DefaultSerializationFormat = "HH:mm:ss.fff";

    private string SerializationFormat { get; init; }

    /// <summary>
    /// Creates a new instance of the <see cref="TimeOnlyJsonConverter"/> class.
    /// </summary>
    public TimeOnlyJsonConverter() : this(DefaultSerializationFormat)
    {
        // Empty.
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TimeOnlyJsonConverter"/> class.
    /// </summary>
    /// <param name="serializationFormat">The format to use when serializing the time value as a string.</param>
    public TimeOnlyJsonConverter(string serializationFormat)
    {
        this.SerializationFormat = serializationFormat ?? DefaultSerializationFormat;
    }

    /// <inheritdoc />
    public override TimeOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return TimeOnly.Parse(value);
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        TimeOnly value,
        JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(SerializationFormat));
}
