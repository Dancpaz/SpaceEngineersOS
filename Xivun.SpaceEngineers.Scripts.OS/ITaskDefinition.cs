using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    public interface ITaskDefinition
    {
        TaskCreator Creator { get; }
        TaskPriorities Priority { get; }

        IScheduleTask CreateTask(TimeSpan targetTime, TaskScheduler scheduler);
    }
}
