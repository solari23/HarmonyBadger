using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HarmonyBadgerFunctionApp.TaskModel;

/// <summary>
/// A schedule based on a CRON expression.
/// </summary>
/// <remarks>
/// See documentation for <see cref="Expression"/> for details on syntax.
/// </remarks>
public class CronSchedule : ISchedule, IValidatableObject, IJsonOnDeserialized
{
    /// <inheritdoc />
    public ScheduleKind ScheduleKind => ScheduleKind.Cron;

    /// <summary>
    /// The CRON expression.
    /// </summary>
    /// <remarks>
    /// CRON expressions are parsed using the NCrontab library.
    /// See here for documentation on expression syntax:
    /// https://github.com/atifaziz/NCrontab/wiki/Crontab-Expression
    /// </remarks>
    public string Expression { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> ToCronExpressions(DateTime? _ = null)
    {
        yield return this.Expression;
    }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (string.IsNullOrWhiteSpace(this.Expression))
        {
            yield return new ValidationResult(
                $"CRON schedule is missing field '{nameof(this.Expression)}'");
        }
    }

    /// <inheritdoc />
    public void OnDeserialized() => this.ThrowIfNotValid();
}
