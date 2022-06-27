using System.Collections.Generic;

namespace HarmonyBadgerFunctionApp.TaskModel;

public class ScheduledTask
{
    public bool IsEnabled { get; set; }

    public List<ISchedule> Schedule { get; set; }
}
