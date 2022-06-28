using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using HarmonyBadgerFunctionApp.TaskModel;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace HarmonyBadgerFunctionApp.Scheduler;

/// <summary>
/// The HarmonyBadger_Scheduler function implements the logic for
/// evaluating <see cref="ScheduledTask"/> configurations and queueing
/// up tasks to be executed when their schedule is due.
/// </summary>
public class SchedulerFunction
{
    private const string HourlyTrigger = "0 0 */1 * * *";

    private const string Every30SecondsTrigger = "*/30 * * * * *";

    private const string Every10SecondsTrigger = "*/10 * * * * *";

    /// <summary>
    /// The entry point for the HarmonyBadger_Scheduler function.
    /// </summary>
    [FunctionName("HarmonyBadger_Scheduler")]
    public async Task RunAsync(
        [TimerTrigger(Every10SecondsTrigger)] TimerInfo myTimer,
        ILogger log,
        ExecutionContext context)
    {
        await Task.Yield();

        TestMethod(context);

        var localTime = TimeHelper.CurrentLocalTime;
        var crontabSchedule = CrontabSchedule.Parse(
            Every30SecondsTrigger,
            new CrontabSchedule.ParseOptions
            {
                IncludingSeconds = true,
            });
        var numOccurrences = crontabSchedule
            .GetNextOccurrences(localTime.AddSeconds(-5), localTime.AddSeconds(5))
            .Count();

        var files = string.Join(", ", Directory.EnumerateFiles(GetTaskConfigDirectoryPath(context)));
        log.LogInformation($"[{DateTime.UtcNow}][L:{localTime}] Timer triggered, [#{numOccurrences}] found config files: {files}");
    }

    private static string GetTaskConfigDirectoryPath(ExecutionContext context)
        => Path.Combine(
            context.FunctionAppDirectory,
            Constants.TaskConfigsDirectoryName);

    // Throwaway code for prototyping while developing. This will be removed in the near future.
    private void TestMethod(ExecutionContext context)
    {
        var jsonText = File.ReadAllText(
            Path.Combine(GetTaskConfigDirectoryPath(context), "Sample.Test1.schedule.json"));
        var foo = JsonSerializer.Deserialize<TaskModel.ScheduledTask>(jsonText);
        var crons = foo.Schedule.SelectMany(s => s.ToCronExpressions()).ToList();
    }
}
