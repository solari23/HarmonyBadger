using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using HarmonyBadgerFunctionApp.TaskModel.Tasks;

namespace HarmonyBadgerFunctionApp.TaskModel;

/// <summary>
/// A JSON converter that allows deserializing an <see cref="ITask"/>
/// object without knowing the exact implementation type ahead of time.
/// </summary>
public class TaskPolymorphicJsonConverter : JsonConverter<ITask>
{
    /// <inheritdoc />
    public override bool CanConvert(Type t) => t.IsAssignableFrom(typeof(ITask));

    /// <inheritdoc />
    public override ITask Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (JsonDocument.TryParseValue(ref reader, out var doc))
        {
            if (doc.RootElement.TryGetProperty(
                nameof(ITask.TaskKind),
                out var kindNode))
            {
                var rawKind = kindNode.GetString();
                if (!Enum.TryParse(rawKind, ignoreCase: true, out TaskKind kind))
                {
                    throw new JsonException($"Value '{rawKind}' is not a valid {nameof(TaskKind)}");
                }

                var rootElement = doc.RootElement.GetRawText();

                return kind switch
                {
                    TaskKind.Test => JsonSerializer.Deserialize<TestTask>(rootElement, options),
                    _ => throw new JsonException($"Task kind '{kind}' is not currently supported"),
                };
            }

            throw new JsonException($"Task is missing required field '{nameof(ITask.TaskKind)}'");
        }

        throw new JsonException("Failed to parse task");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ITask value, JsonSerializerOptions options)
    {
        var serialiationType = value.TaskKind switch
        {
            TaskKind.Test => typeof(TestTask),
            _ => throw new JsonException($"Task kind '{value.TaskKind}' is not currently supported"),
        };

        JsonSerializer.Serialize(writer, value, serialiationType);
    }
}
