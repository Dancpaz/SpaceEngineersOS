using System;
using System.Collections.Generic;
using System.Text;

using IngameScript.Containers;

namespace IngameScript
{
    public class TaskScheduler
    {
        long LastOrdinal;

        List<IScheduleTask> Suspended;
        IQueue<IScheduleTask> Pending;
        IQueue<IScheduleTask> Active;
        
        public CapacityManager Capacity { get; }

        public TimeSpan CurrentTime => DateTime.UtcNow - DateTime.MinValue;
        public int CurrentTick { get; private set; }

        public int TaskCount => Pending.Count + Active.Count;

        public TaskScheduler(Func<IComparer<IScheduleTask>, IQueue<IScheduleTask>> queueFactory, CapacityManager capacity)
        {
            Suspended = new List<IScheduleTask>();
            Pending = queueFactory(new ScheduleTaskTimeComparer());
            Active = queueFactory(new ScheduleTaskActiveComparer());
            Capacity = capacity;

            LastOrdinal = 0;

            UpdateTime();
        }

        public bool HasCapacity() => Capacity.HasCapacity(CurrentTime);

        public IScheduleTask Schedule(ITaskDefinition definition, TimeSpan targetTime)
        {
            return Schedule(definition.CreateTask(targetTime, this), null);
        }
        public IScheduleTask Schedule<T>(TaskCreator<T> creator, TaskPriorities priority, TimeSpan targetTime)
        {
            return Schedule(new TaskDefinition<T>
            {
                Creator = creator,
                Priority = priority
            }, targetTime);
        }
        public IScheduleTask Schedule(IScheduleTask task, TimeSpan? targetElapsed)
        {
            if (task.Completed)
                throw new InvalidOperationException();

            task.TargetTime = targetElapsed ?? task.TargetTime;
            task.PriorityOrdinal = ++LastOrdinal + (long)Math.Pow(2, LastOrdinal);

            var queue = task.TargetTime <= Capacity.UpdateTime ? Active : Pending;

            task.Running = true;
            queue.Enqueue(task);
            return task;
        }

        public void Update()
        {
            CurrentTick++;

            UpdateSuspended();

            UpdateTime();

            UpdatePending();
            UpdateActive();
        }

        public void Run(IScheduleTask task)
        {
            if (task.Running)
                throw new InvalidOperationException();

            task.Running = true;
            DoRun(task);
        }

        private void DoRun(IScheduleTask task)
        {
            ITaskYield result;
            while(true)
            {
                // if no capacity, reschedule for next cycle @ same time/pos
                if(!HasCapacity())
                {
                    Schedule(task, null);
                    break;
                }

                // advance task
                bool advanced;
                try 
                {
                    advanced = task.Enumerator.MoveNext();

                    result = advanced
                        ? task.Enumerator.Current
                        : null;
                }
                catch(Exception ex)
                {
                    OnTaskException(task, ex);
                    return;
                }

                // quit if the task is complete
                if(!advanced || result.Command == YieldCommands.Success)
                {
                    OnTaskSucceeded(task, result);
                    break;
                }

                switch (result.Command)
                {
                    case YieldCommands.Check:
                        // performance check will occur on the next iteration
                        continue;

                    case YieldCommands.Suspend:
                        // will resume on the next update
                        Suspended.Add(task);
                        return;

                    case YieldCommands.Yield:
                        // back into pending, after all other tasks at same priority
                        Schedule(task, Capacity.UpdateTime);
                        return;

                    case YieldCommands.Delay:
                        Schedule(task, CurrentTime + (result.DelayTime ?? TimeSpan.Zero));
                        return;

                    case YieldCommands.Await:
                        // if a definition is passed, create a task to await
                        IScheduleTask awaiting = null;
                        if (result.Await != null)
                        {
                            awaiting = result.Await.CreateTask(task.TargetTime, this);
                        }

                        // or if a task was passed, prepare to await it
                        if (result.AwaitTask != null)
                        {
                            // if already complete, immediately resume original task
                            if (result.AwaitTask.Completed)
                            {
                                Run(task);
                                return;
                            }

                            awaiting = result.AwaitTask;
                        }

                        // if we have a task prepared to await...
                        if (awaiting != null)
                        {
                            // assign continuations
                            if (awaiting.Continuations == null)
                                awaiting.Continuations = new List<IScheduleTask> { task };
                            else
                                awaiting.Continuations.Add(task);

                            // and start the task if necessary
                            if (!awaiting.Running)
                                awaiting.Start();

                            return;
                        }
                        return;

                    default:
                        // bad value or not valid at this point in the code
                        throw new InvalidOperationException($"Invalid value: {result.Command}");
                }
            }
        }

        void UpdateSuspended()
        {
            var time = CurrentTime;

            foreach (var task in Suspended)
                Schedule(task, time);

            Suspended.Clear();
        }

        /// <summary>
        /// Moves pending items into the active queue once their scheduled time has arrived.
        /// </summary>
        void UpdatePending()
        {
            while (Pending.Count > 0 && Pending.Peek().TargetTime <= Capacity.UpdateTime)
                Active.Enqueue(Pending.Dequeue());
        }

        /// <summary>
        /// Processes items in the active queue so long as we have capacity this cycle.
        /// </summary>
        void UpdateActive()
        {
            // process tasks in the main queue until we run out of capacity
            IScheduleTask task;
            while (Capacity.HasCapacity(CurrentTime) && Active.Count > 0)
            {
                task = Active.Dequeue();

                // TODO: capture and handle exceptions
                DoRun(task);
            }
        }


        //public TimeSpan ApplyResolution(TimeSpan input)
        //{
        //    return TimeSpanHelper.FromMilliseconds(Math.Ceiling(input.TotalMilliseconds / Resolution.TotalMilliseconds) * Resolution.TotalMilliseconds);
        //}

        void UpdateTime()
        {
            Capacity.Update(CurrentTime);
        }

        void OnTaskSucceeded(IScheduleTask task, ITaskYield yield)
        {
            task.SetResult(yield?.Result);
            OnTaskCompleted(task);
        }

        void OnTaskException(IScheduleTask task, Exception ex)
        {
            // TODO: Log exceptions
            task.SetException(ex);
            OnTaskCompleted(task);

            // if continuations did not mark that the exception was handled,
            // we need to throw it again
            if (!task.ExceptionHandled)
                throw task.Exception;
        }

        void OnTaskCompleted(IScheduleTask task)
        {
            if (task.Continuations != null)
                foreach (var continuation in task.Continuations)
                {
                    continuation.State.AwaitedResult = task.Result;

                    // TODO: consider benefits/risks of scheduling continuations instead of running directly
                    DoRun(continuation);
                }
        }
    }




    


}
