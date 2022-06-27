namespace HarmonyBadgerFunctionApp.TaskModel;

public enum ScheduleKind
{
    Invalid = 0,

    Cron = 1,
    Daily = 2,
    Weekly = 3,
    LastDayOfMonth = 4,
}
