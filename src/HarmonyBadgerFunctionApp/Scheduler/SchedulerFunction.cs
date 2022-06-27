using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCrontab;
using TimeZoneConverter;

namespace HarmonyBadgerFunctionApp.Scheduler;

public class SchedulerFunction
{
    private const string HourlyTrigger = "0 0 */1 * * *";

    private const string Every30SecondsTrigger = "*/30 * * * * *";

    private const string Every10SecondsTrigger = "*/10 * * * * *";

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

    public static string GetTaskConfigDirectoryPath(ExecutionContext context)
        => Path.Combine(
            context.FunctionAppDirectory,
            Constants.TaskConfigsDirectoryName);

    public void TestMethod(ExecutionContext context)
    {
        var jsonText = File.ReadAllText(
            Path.Combine(GetTaskConfigDirectoryPath(context), "Sample.Test1.schedule.json"));
        var foo = JsonSerializer.Deserialize<TaskModel.ScheduledTask>(jsonText);
        var crons = foo.Schedule.SelectMany(s => s.ToCronExpressions()).ToList();
    }
}
