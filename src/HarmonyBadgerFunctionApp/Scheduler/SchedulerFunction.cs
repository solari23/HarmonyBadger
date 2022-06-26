using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace HarmonyBadgerFunctionApp.Scheduler;

public class SchedulerFunction
{
    private const string HourlyTrigger = "0 0 */1 * * *";

    private const string Every10SecondsTrigger = "*/10 * * * * *";

    [FunctionName("HarmonyBadger_Scheduler")]
    public void Run(
        [TimerTrigger(Every10SecondsTrigger)] TimerInfo myTimer,
        ILogger log)
    {
        var files = string.Join(", ", Directory.EnumerateFiles(Constants.TaskConfigsDirectoryName));
        log.LogInformation($"[{DateTime.Now}] Timer triggered, found config files: {files}");
    }
}
