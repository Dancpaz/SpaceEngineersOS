using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    class TaskWrapper
    {
        /// <summary>
        /// Wraps a task with other sub-tasks that run before and/or after every step.
        /// </summary>
        /// <param name="task">The main task to run.  This will be advanced each update.</param>
        /// <param name="beforeStep">Subtask to run to completion before every update.  Capable of completing, awaiting, or delaying the main task.  For example, may check for nearby hostile entities and fire off other tasks when found, while suspending or even aborting the main task.</param>
        /// <param name="afterStep">Subtask to process the output of every advancement.  Capable of completing, awaiting, or delaying the main task.  Capable of altering its outputs.</param>
        /// <returns>A TaskCreator<typeparamref name="T"/> that combined the supplied task with any provided subtasks.</returns>
        public TaskCreator<T> Create<T>(TaskCreator<T> task, TaskCreator<T> beforeStep = null, Func<TaskYield<T>, TaskCreator<T>> afterStep = null)
        {
            var taskWrapper = new TaskWrapper<T>(task, beforeStep, afterStep);
            return taskWrapper.Creator;
        }
    }

    class TaskWrapper<T>
    {

        TaskCreator<T> BeforeStep;
        TaskCreator<T> Task;
        Func<TaskYield<T>, TaskCreator<T>> AfterStep;

        /// <summary>
        /// Wraps a task with other sub-tasks that run before and/or after every step.
        /// </summary>
        /// <param name="task">The main task to run.  This will be advanced each update.</param>
        /// <param name="beforeStep">Subtask to run to completion before every update.  Capable of completing, awaiting, or delaying the main task.</param>
        /// <param name="afterStep">Subtask to process the output of every advancement.  Capable of completing, awaiting, or delaying the main task.  Capable of altering its outputs.</param>
        public TaskWrapper(TaskCreator<T> task, TaskCreator<T> beforeStep, Func<TaskYield<T>, TaskCreator<T>> afterStep)
        {
            Task = task;
            BeforeStep = beforeStep;
            AfterStep = afterStep;
        }
        
        public IEnumerable<TaskYield<T>> Creator(TaskState<T> state)
        {
            var beforeStep = BeforeStep?.Invoke(state);
            var enumerator = Task(state).GetEnumerator();
            
            while(true)
            {
                if (beforeStep != null)
                    foreach (var yielded in beforeStep)
                        yield return yielded;

                if (!enumerator.MoveNext())
                    yield break;

                if (AfterStep != null)
                    foreach (var yielded in AfterStep(enumerator.Current)(state))
                        yield return yielded;
                else
                    yield return enumerator.Current;
            }
        }

    }

    delegate bool ConditionalTaskCreator(out TaskCreator creator);
}
