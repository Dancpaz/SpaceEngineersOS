using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

using SpaceEngineers.Game.ModAPI.Ingame;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;

using VRageMath;

namespace IngameScript
{
    public class MainTask
    {
        MyGridProgram Program;

        public Dictionary<TaskPriorities, TaskRunStatistics> PriorityStatistics;

        public MainTask(MyGridProgram program)
        {
            PriorityStatistics = new Dictionary<TaskPriorities, TaskRunStatistics>();
            Program = program;
        }

        void Log(string text) => Program.Echo(text);


        void Something<T1, T2>(Func<T1, T2, IEnumerable<TaskYield<T2>>> creator)
        {

        }

        public IEnumerable<TaskYield<object>> Run(TaskState<object> state)
        {
            Action<string> log = text => Log($"MainTask:  {text}");

            // TODO: separate testing script project from example script project

            // number of tasks to spin up for testing
            var testTaskCount = 125;

            // spin up tasks
            var testTasks = new List<IScheduleTask>();
            for (int i = 0; i < testTaskCount; i++)
                testTasks.Add(state.Run<object>(TestLoopTask, (TaskPriorities)(i % 5)));

            // ordinary example loop - disabled for performance.  disabled temporarily.
            bool quit = false;
            while (!quit)
            {
                //// run through a sub task method with the same return type without awaiting.  Doing it this way
                //// allows the sub task to essentially act as part of the main task: it can complete your task, etc.
                //foreach (var yielded in SubTaskOne(state))
                //    yield return yielded;

                //// let the scheduler run other tasks of the same/higher priority, then come back this update if capacity remains
                //log("Started.  Yielding.");
                //yield return state.Yield();

                //// scheduler performs a quick capacity check and returns immediately if capacity remains
                //log("Yielded.  Checking.");
                //yield return state.Check();

                //// scheduler suspends task for 5 seconds, then resumes
                ////log("Checked.  Delaying for 5 second.");
                ////yield return state.Delay(TimeSpan.FromSeconds(5));

                //// scheduler creates and runs task for the supplied sub task method,
                //// and it does not resume the current task until the awaited task is completed
                //log("Delayed.  Awaiting SubTask.");
                //yield return state.Await<int>(SubTaskTwo);
                //log($"{state.AwaitedResult}");              // a way to get access to the results of the awaited task

                //// another form of await
                //var task = state.Run<int>(SubTaskTwo);          // scheduler runs the supplied task and then comes back
                //if (task.Success)                               // handle result of awaited task
                //{
                //    log($"{task.Result}");                      // respond to result of successful task
                //}
                //else
                //{
                //    SomeExceptionHandlerCode(task.Exception);   // respond to failure of task
                //    task.ExceptionHandled = true;               // let scheduler know the exception is handled
                //}

                //// scheduler suspends task until the next PB run
                //log($"Resumed from await.  Result = {state.AwaitedResult}.  Completing.");
                // yield return state.Yield();
                yield return state.Sleep();
            }

            // although this will never happen, await all the tasks we spun up
            foreach(var testTask in testTasks)
                yield return state.Await(testTask);
        }

        IEnumerable<TaskYield<object>> TestLoopTask(TaskState<object> state)
        {
            TaskRunStatistics stats;
            if(!PriorityStatistics.TryGetValue(state.Priority, out stats))
            {
                stats = new TaskRunStatistics();
                PriorityStatistics.Add(state.Priority, stats);
            }

            // add ourselves to the task count for our priority
            ++stats.TaskCount;

            while(true)
            {
                ++stats.RunCount;               // increment run count for our priority
                yield return state.Yield();     // yield to other tasks
            }
        }

        public class TaskRunStatistics
        {
            public int TaskCount { get; set; }
            public int RunCount { get; set; }
        }

        IEnumerable<TaskYield<object>> SubTaskOne(TaskState<object> state)
        {
            for(int i = 0; i < 10; i++)
            {
                // do stuff;
                yield return state.Yield();
            }
        }

        IEnumerable<TaskYield<int>> SubTaskTwo(TaskState<int> state)
        {
            Action<string> log = text => Log($"SubTask:  {text}");

            log("Started.  Yielding.");
            yield return state.Yield();

            //log($"Resumed from Yield.  Delaying for 1 seconds.");
            //yield return state.Delay(TimeSpan.FromSeconds(1));

            log($"Resumed from Delay.  Returning 5.");

            yield return state.Success(5);
            log($"Completed.");
        }


        void SomeExceptionHandlerCode(Exception ex)
        {

        }
    }
}
