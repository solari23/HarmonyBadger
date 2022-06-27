using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HarmonyBadgerFunctionApp.TaskModel;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private string SerializationFormat { get; init; }

    public TimeOnlyJsonConverter() : this(null)
    {
        // Empty.
    }

    public TimeOnlyJsonConverter(string serializationFormat)
    {
        this.SerializationFormat = serializationFormat ?? "HH:mm:ss.fff";
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
