using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    public class TaskYield<T> : ITaskYield
    {
        public YieldCommands Command { get; set; }
        public TimeSpan? DelayTime { get; set; }
        public long? DelayTicks { get; set; }

        
        public T Result { get; set; }

        public ITaskDefinition Await { get; set; }
        public IScheduleTask AwaitTask { get; set; }

        object ITaskYield.Result => Result;
    }
}
