using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HarmonyBadgerFunctionApp.TaskModel;

/// <summary>
/// A JSON converter that allows deserializing an <see cref="ISchedule"/>
/// object without knowing the exact implementation type ahead of time.
/// </summary>
public class SchedulePolymorphicJsonConverter : JsonConverter<ISchedule>
{
    /// <inheritdoc />
    public override bool CanConvert(Type t) => t.IsAssignableFrom(typeof(ISchedule));

    /// <inheritdoc />
    public override ISchedule Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (JsonDocument.TryParseValue(ref reader, out var doc))
        {
            if (doc.RootElement.TryGetProperty(
                nameof(ISchedule.ScheduleKind),
                out var kindNode))
            {
                var rawKind = kindNode.GetString();
                if (!Enum.TryParse(rawKind, ignoreCase: true, out ScheduleKind kind))
                {
                    throw new JsonException($"Value '{rawKind}' is not a valid {nameof(ScheduleKind)}");
                }

                var rootElement = doc.RootElement.GetRawText();

                return kind switch
                {
                    ScheduleKind.Cron => JsonSerializer.Deserialize<CronSchedule>(rootElement, options),
                    ScheduleKind.Daily => JsonSerializer.Deserialize<DailySchedule>(rootElement, options),
                    ScheduleKind.Weekly => JsonSerializer.Deserialize<WeeklySchedule>(rootElement, options),
                    ScheduleKind.LastDayOfMonth => JsonSerializer.Deserialize<LastDayOfMonthSchedule>(rootElement, options),
                    _ => throw new JsonException($"Schedule kind '{kind}' is not currently supported"),
                };
            }

            throw new JsonException($"Schedule is missing required field '{nameof(ISchedule.ScheduleKind)}'");
        }

        throw new JsonException("Failed to parse schedule");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ISchedule value, JsonSerializerOptions options)
        => throw new NotImplementedException();  // No current use-cases to write JSON schedules.
}
