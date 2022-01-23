using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    public interface ITaskState
    {
        object AwaitedResult { get; set; }
        TaskScheduler Scheduler { get; }
        bool HasCapacity();
    }
}
