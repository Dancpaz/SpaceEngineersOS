using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    public interface IScheduleTask
    {
        /// <summary>
        /// Used to suspend/resume tasks using a time schedule.
        /// </summary>
        TimeSpan TargetTime { get; set; }

        /// <summary>
        /// Used to suspend/resume tasks using an internal tick schedule.
        /// </summary>
        long TargetTick { get; set; }

        /// <summary>
        /// Used to schedule active tasks, ensuring that everything gets time, but higher priority tasks get *more* time.
        /// </summary>
        long PriorityOrdinal { get; set; }

        TaskPriorities Priority { get; }
        

        IEnumerator<ITaskYield> Enumerator { get; }
        ITaskState State { get; }


        bool Running { get; set; }
        bool Completed { get; }
        bool Success { get; }
        object Result { get; }
        Exception Exception { get; }
        bool ExceptionHandled { get; set; }

        IList<IScheduleTask> Continuations { get; set; }

        void SetResult(object result);
        void SetException(Exception exception);
        void Start();
    }

    //public interface IScheduleTask<T> : IScheduleTask
    //{
    //    new IEnumerator<TaskYield<T>> Enumerator { get; }
    //    new T Result { get; }

    //    void SetResult(T result);
    //}
}
