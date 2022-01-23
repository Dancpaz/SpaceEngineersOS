using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    public class TaskDefinition<T> : ITaskDefinition
    {
        public TaskCreator<T> Creator { get; set; }
        public TaskPriorities Priority { get; set; }

        TaskCreator ITaskDefinition.Creator => state => Creator((TaskState<T>)state);

        public IScheduleTask CreateTask(TimeSpan targetTime, TaskScheduler scheduler)
        {
            var state = new TaskState<T>
            {
                Scheduler = scheduler,
                Priority = Priority
            };

            return new ScheduleTask<T>
            {
                Priority = Priority,
                Enumerator = Creator(state).GetEnumerator(),
                TargetTime = targetTime,
                State = state
            };
        }
    }
}
