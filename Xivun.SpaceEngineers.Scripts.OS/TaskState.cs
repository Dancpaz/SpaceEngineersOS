using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    public class TaskState<T> : ITaskState
    {
        public object AwaitedResult { get; set; }
        public TaskScheduler Scheduler { get; set; }
        public TaskPriorities Priority { get; set; }

        public bool HasCapacity() => Scheduler.HasCapacity();

        public TaskYield<T> Success(T value) => new TaskYield<T>
        {
            Command = YieldCommands.Success,
            Result = value
        };

        public TaskYield<T> Await(ITaskDefinition definition) => new TaskYield<T>
        {
            Command = YieldCommands.Await,
            Await = definition
        };

        public TaskYield<T> Await<T2>(TaskCreator<T2> creator, TaskPriorities? priority = null) => new TaskYield<T>
        {
            Command = YieldCommands.Await,
            Await = new TaskDefinition<T2>
            {
                Creator = creator,
                Priority = priority ?? Priority
            }
        };

        public TaskYield<T> Await(IScheduleTask task) => new TaskYield<T>
        {
            Command = YieldCommands.Await,
            AwaitTask = task
        };

        public ScheduleTask<T2> Run<T2>(TaskCreator<T2> method, TaskPriorities? priority = null)
        {
            var definition = new TaskDefinition<T2>
            {
                Creator = method,
                Priority = priority ?? Priority
            };

            var task = (ScheduleTask<T2>)definition.CreateTask(Scheduler.CurrentTime, Scheduler);

            Scheduler.Run(task);

            return task;
        }

        public TaskYield<T> Delay(TimeSpan duration) => new TaskYield<T>
        {
            Command = YieldCommands.Delay,
            DelayTime = duration
        };

        public TaskYield<T> Yield() => new TaskYield<T>
        {
            Command = YieldCommands.Yield,
        };

        public TaskYield<T> Check() => new TaskYield<T>
        {
            Command = YieldCommands.Check,
        };

        public TaskYield<T> Suspend() => new TaskYield<T>
        {
            Command = YieldCommands.Suspend,
        };
    }


}
