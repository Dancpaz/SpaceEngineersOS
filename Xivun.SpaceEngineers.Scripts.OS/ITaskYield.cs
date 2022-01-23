using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{

    public interface ITaskYield
    {
        YieldCommands Command { get; }
        TimeSpan? DelayTime { get; }
        long? DelayTicks { get; }
        
        object Result { get; }
        ITaskDefinition Await { get; }
        IScheduleTask AwaitTask { get; }
    }

}
