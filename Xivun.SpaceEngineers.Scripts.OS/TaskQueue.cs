using System;
using System.Collections.Generic;
using System.Text;

using IngameScript.Containers;

namespace IngameScript
{
    class TaskQueue
    {
        IQueue<IScheduleTask> PendingTime;
        IQueue<IScheduleTask> PendingTick;
        IQueue<IScheduleTask> Active;

        public int Count => PendingTime.Count + PendingTick.Count + Active.Count;
        public int ActiveCount => Active.Count;

        public TaskQueue(Func<IComparer<IScheduleTask>, IQueue<IScheduleTask>> queueFactory)
        {
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
            UpdatePendingTime(updateTime);
            UpdatePendingTick(internalTick);
        }

        public IScheduleTask Dequeue() => Active.Dequeue();
        public bool TryDequeue(out IScheduleTask task) => Active.TryDequeue(out task);

        void UpdatePendingTime(TimeSpan updateTime)
        {
            while (PendingTime.Count > 0 && PendingTime.Peek().TargetTime <= updateTime)
                Active.Enqueue(PendingTime.Dequeue());
        }

        void UpdatePendingTick(long internalTick)
        {
            while (PendingTick.Count > 0 && PendingTick.Peek().TargetTick <= internalTick)
                Active.Enqueue(PendingTick.Dequeue());
        }

    }
}
