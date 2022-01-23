using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    public class ScheduleTask<T> : IScheduleTask
    {
        public TimeSpan TargetTime { get; set; }
        public long TargetTick { get; set; }
        public long PriorityOrdinal { get; set; }
        public TaskPriorities Priority { get; set; }
        

        public bool Running { get; set; }

        public IEnumerator<TaskYield<T>> Enumerator { get; set; }
        public ITaskState State { get; set; }
        public bool Completed { get; set; }
        public bool Success { get; set; }
        public T Result { get; set; }
        public Exception Exception { get; set; }
        public bool ExceptionHandled { get; set; }

        public IList<IScheduleTask> Continuations { get; set; }

        IEnumerator<ITaskYield> IScheduleTask.Enumerator => Enumerator;
        object IScheduleTask.Result => Result;

        public void Start()
        {
            if (Running | Completed)
                throw new InvalidOperationException();

            State.Scheduler.Run(this);
        }

        public void SetResult(T result)
        {
            Running = false;
            Completed = true;
            Success = true;
            Result = result;
        }
        void IScheduleTask.SetResult(object result) => SetResult((T)result);

        public void SetException(Exception exception)
        {
            Running = false;
            Completed = true;
            Success = false;
            Exception = exception;
        }
    }
}
