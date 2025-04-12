using System.Text.Json;
using System.Text.Json.Serialization;

namespace HarmonyBadger.ConfigModels;

/// <summary>
/// A JSON converter that allows serializing and deserializing <see cref="DateOnly"/> objects.
/// </summary>
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    /// <summary>
    /// The default format for serializing <see cref="DateOnly"/> values as strings.
    /// </summary>
    public const string DefaultSerializationFormat = "yyyy-MM-dd";

    private string SerializationFormat { get; init; }

    /// <summary>
    /// Creates a new instance of the <see cref="DateOnlyJsonConverter"/> class.
    /// </summary>
    public DateOnlyJsonConverter() : this(DefaultSerializationFormat)
    {
        // Empty.
    }

    /// <summary>
    /// Creates a new instance of the <see cref="DateOnlyJsonConverter"/> class.
    /// </summary>
    /// <param name="serializationFormat">The format to use when serializing the date value as a string.</param>
    public DateOnlyJsonConverter(string serializationFormat)
    {
        this.SerializationFormat = serializationFormat ?? DefaultSerializationFormat;
    }

    /// <inheritdoc />
    public override DateOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return DateOnly.Parse(value);
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        DateOnly value,
        JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(SerializationFormat));
}
