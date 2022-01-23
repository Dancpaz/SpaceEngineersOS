using System;
using System.Collections.Generic;
using System.Text;

using IngameScript.Containers;

namespace IngameScript
{
    class TaskQueue
    {
        List<IScheduleTask> Suspended;
        IQueue<IScheduleTask> PendingTime;
        IQueue<IScheduleTask> PendingTick;
        IQueue<IScheduleTask> Active;

        TimeSpan UpdateTime;
        long InternalTick;

        public int Count => Suspended.Count + PendingTime.Count + PendingTick.Count + Active.Count;
        public int ActiveCount => Active.Count;

        public TaskQueue(Func<IComparer<IScheduleTask>, IQueue<IScheduleTask>> queueFactory)
        {
            Suspended = new List<IScheduleTask>();
            PendingTime = queueFactory(new ScheduleTaskTimeComparer());
            PendingTick = queueFactory(new ScheduleTaskTickComparer());
            Active = queueFactory(new ScheduleTaskActiveComparer());
        }

        /// <summary>
        /// Moves items from pending queues into the active queue on/after their scheduled time or tick.
        /// </summary>
        /// <param name="internalTick"></param>
        /// <param name="updateTime"></param>
        public void Update(long internalTick, TimeSpan updateTime)
        {
            InternalTick = internalTick;
            UpdateTime = updateTime;

            UpdateSuspended();
            UpdatePendingTime();
            UpdatePendingTick();
        }

        public void Enqueue(IScheduleTask task)
        {
            if (task.TargetTime > UpdateTime)
            {
                PendingTime.Enqueue(task);
                return;
            }

            var ticks = task.TargetTick - InternalTick;

            if (ticks <= 0)
            {
                Active.Enqueue(task);
            }
            else if (ticks == 1)
                Suspended.Add(task);
            else
                PendingTick.Enqueue(task);
        }

        public IScheduleTask Dequeue() => Active.Dequeue();
        public bool TryDequeue(out IScheduleTask task) => Active.TryDequeue(out task);


        void UpdateSuspended()
        {
            foreach (var task in Suspended)
                Active.Enqueue(task);
            Suspended.Clear();
        }

        /// <summary>
        /// Moves tasks out of PendingTime into another queue as appropriate.
        /// </summary>
        void UpdatePendingTime()
        {
            while (PendingTime.Count > 0 && PendingTime.Peek().TargetTime <= UpdateTime)
               Active.Enqueue(PendingTime.Dequeue());
        }

        /// <summary>
        /// Moves tasks out of PendingTick into another queue as appropriate.
        /// </summary>
        void UpdatePendingTick()
        {
            while (PendingTick.Count > 0 && PendingTick.Peek().TargetTick <= InternalTick)
                Active.Enqueue(PendingTick.Dequeue());
        }

    }
}
