using System;
using System.Linq;
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
    /// Creates a new instance of the <see cref="SchedulerFunction"/> class.
    /// </summary>
    public SchedulerFunction(IScheduledTaskConfigLoader taskConfigLoader)
    {
        this.TaskConfigLoader = taskConfigLoader;
    }

    private IScheduledTaskConfigLoader TaskConfigLoader { get; }

    /// <summary>
    /// The entry point for the HarmonyBadger_Scheduler function.
    /// </summary>
    [FunctionName("HarmonyBadger_Scheduler")]
    public async Task RunAsync(
        [TimerTrigger(Every30SecondsTrigger)] TimerInfo myTimer,
        ILogger log,
        ExecutionContext context)
    {
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

        var configs = await this.TaskConfigLoader.LoadScheduledTasksAsync(log, context);
        log.LogInformation($"[{DateTime.UtcNow}][L:{localTime}] Timer triggered, [#{numOccurrences}] found {configs.Count} config files");
    }
}
