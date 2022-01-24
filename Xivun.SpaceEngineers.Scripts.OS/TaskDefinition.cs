using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    public class TaskDefinition<TResult> : ITaskDefinition
    {
        public TaskCreator<TResult> Creator { get; set; }
        public TaskPriorities Priority { get; set; }

        TaskCreator ITaskDefinition.Creator => state => Creator((TaskState<TResult>)state);

        public IScheduleTask CreateTask(TimeSpan targetTime, TaskScheduler scheduler)
        {
            var state = new TaskState<TResult>
            {
                Scheduler = scheduler,
                Priority = Priority
            };

            return new ScheduleTask<TResult>
            {
                Priority = Priority,
                Enumerator = Creator(state).GetEnumerator(),
                TargetTime = targetTime,
                State = state
            };
        }
    }

    public class TaskDefinition<TArg, TResult> : ITaskDefinition
    {
        public TaskCreator<TArg, TResult> Creator { get; set; }
        public TaskPriorities Priority { get; set; }
        public TArg Arg { get; set; }

        TaskCreator ITaskDefinition.Creator => state => Creator((TaskState<TResult>)state, Arg);

        public IScheduleTask CreateTask(TimeSpan targetTime, TaskScheduler scheduler)
        {
            var state = new TaskState<TResult>
            {
                Scheduler = scheduler,
                Priority = Priority
            };

            return new ScheduleTask<TResult>
            {
                Priority = Priority,
                Enumerator = Creator(state, Arg).GetEnumerator(),
                TargetTime = targetTime,
                State = state
            };
        }
    }
}
