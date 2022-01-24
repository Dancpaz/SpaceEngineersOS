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

        public TaskYield<T> Await<TArg, TResult>(TaskCreator<TArg, TResult> creator, TArg arg, TaskPriorities? priority = null) => new TaskYield<T>
        {
            Command = YieldCommands.Await,
            Await = new TaskDefinition<TArg, TResult>
            {
                Creator = creator,
                Arg = arg,
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

        public ScheduleTask<TResult> Run<TArg, TResult>(TaskCreator<TArg, TResult> method, TArg arg, TaskPriorities? priority = null)
        {
            var definition = new TaskDefinition<TArg, TResult>
            {
                Creator = method,
                Arg = arg,
                Priority = priority ?? Priority
            };

            var task = (ScheduleTask<TResult>)definition.CreateTask(Scheduler.CurrentTime, Scheduler);

            Scheduler.Run(task);

            return task;
        }

        public TaskYield<T> Sleep(TimeSpan duration) => new TaskYield<T>
        {
            Command = YieldCommands.SleepTime,
            DelayTime = duration
        };

        public TaskYield<T> Sleep(long ticks = 1) => new TaskYield<T>
        {
            Command = YieldCommands.SleepTicks,
            DelayTicks = ticks
        };

        public TaskYield<T> Yield() => new TaskYield<T>
        {
            Command = YieldCommands.Yield,
        };

        public TaskYield<T> Check() => new TaskYield<T>
        {
            Command = YieldCommands.Check,
        };
    }


}
