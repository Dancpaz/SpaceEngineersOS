using System;
using System.Collections.Generic;
using System.Text;

using IngameScript.Containers;

namespace IngameScript
{
    public class TaskScheduler
    {
        long LastOrdinal;

        TaskQueue Tasks;
        
        public CapacityManager Capacity { get; }

        public TimeSpan CurrentTime => DateTime.UtcNow - DateTime.MinValue;
        public int CurrentTick { get; private set; }

        public int TaskCount => Tasks.Count;

        public TaskScheduler(Func<IComparer<IScheduleTask>, IQueue<IScheduleTask>> queueFactory, CapacityManager capacity)
        {
            Tasks = new TaskQueue(queueFactory);
            Capacity = capacity;

            LastOrdinal = 0;

            UpdateTime();
        }

        public bool HasCapacity() => Capacity.HasCapacity();

        public IScheduleTask Schedule(ITaskDefinition definition, TimeSpan targetTime)
        {
            return Schedule(definition.CreateTask(targetTime, this), null, null);
        }
        public IScheduleTask Schedule<T>(TaskCreator<T> creator, TaskPriorities priority, TimeSpan targetTime)
        {
            return Schedule(new TaskDefinition<T>
            {
                Creator = creator,
                Priority = priority
            }, targetTime);
        }
        public IScheduleTask Schedule(IScheduleTask task, TimeSpan? targetElapsed, long? targetTick)
        {
            if (task.Completed)
                throw new InvalidOperationException();

            task.TargetTime = targetElapsed ?? task.TargetTime;     // for scheduling by time
            task.TargetTick = targetTick ?? task.TargetTick;        // for scheduling by tick
            task.PriorityOrdinal = CalculatePriorityOrdinal(task);  // for prioritization while active

            task.Running = true;
            
            Tasks.Enqueue(task);
            
            return task;
        }

        long CalculatePriorityOrdinal(IScheduleTask task)
        {
            // Higher priority tasks should run more frequently, but all tasks should eventually run.
            // In theory, this should roughly allow each priority level to run roughly twice as often
            // as the priority level below it if there is contention for resources.
            // In practice, the higher priority levels are getting much more time than expected.  This
            // is not what I intended, but it is acceptable, so it is a lower priority fix.
            return ++LastOrdinal + (long)(Math.Pow(2, (int)task.Priority)) * (Tasks.Count + 1);
        }

        public void Update()
        {
            UpdateTime();
            Tasks.Update(++CurrentTick, Capacity.UpdateTime);

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
                // if no capacity, reschedule for next cycle @ same time/tick
                if(!HasCapacity())
                {
                    Schedule(task, null, null);
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

                    case YieldCommands.Yield:
                        // back into pending, after all other tasks at same priority
                        Schedule(task, null, null);
                        return;

                    case YieldCommands.SleepTicks:
                        Schedule(task, null, CurrentTick + (result.DelayTicks ?? 0));
                        return;

                    case YieldCommands.SleepTime:
                        Schedule(task, CurrentTime + (result.DelayTime ?? TimeSpan.Zero), null);
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

        /// <summary>
        /// Processes items in the active queue so long as we have capacity this cycle.
        /// </summary>
        void UpdateActive()
        {
            // process tasks in the main queue until we run out of capacity
            IScheduleTask task;
            while (Capacity.HasCapacity() && Tasks.TryDequeue(out task))
            {
                DoRun(task);
            }
        }

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
