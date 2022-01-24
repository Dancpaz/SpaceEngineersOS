# SpaceEngineersOS
MDK mixin providing a very basic, barebones operating system for your programmable blocks.  Built to keep you below resource consumption targets while enabling convenient patterns like async/await.

Instead of writing basic methods that run every time the Programmable Block is activated, you write enumerator methods, which the scheduler advances under the following conditions:
* The scheduler thinks it can continue without going over the average tick ms limit
* The task is not scheduled to run at a future time
* The task is not scheduled to run on a future tick
* There are tasks scheduled to run before it

If two tasks are scheduled for/at the same time, the task with higher priority will be scheduled to run first.  Higher priority tasks are scheduled to run more often than lower priority tasks, so while all tasks will be processed over time--nothing ever stops for long--higher priority tasks will get significantly more processing time.

# Status
The OS/Scheduler is unfinished.  It works, but the edges are very rough.  Its ability to measure performance requires refinement, and it currently shows a little too much favor to higher priority tasks.  The code could use some cleanup, and in an effort to adapt to the character limit for Space Engineers programmable blocks, I did not follow normal conventions.  Things that would ordinarily be properties are fields--yet still named like properties.  Things that should be read-only are writeable.  The minifier in MDK can do quite a bit, but keywords and library type names remain unaltered, so many of them are intentionally left out.

# Details
The scheduler allows you to run any number of tasks consecutively, limited only by performance.  It will run what it can, when it can, within the average tick time you provide for it.

The scheduler currently supports:
* Tasks with no parameters other than `state`
* Tasks with one parameter other than `state`
* `state.Run(task method)` to create and start a task on the scheduler and retrieve a reference to it
* The ability to communicate to the scheduler by using your task's `state` parameter with `yield return` as follows:
  - `yield return state.Check()` to return immediately if further capacity is available
  - `yield return state.Yield()` to allow other tasks with the same/higher priority to run, before resuming if capacity permits
  - `yield return state.Sleep()` to suspend your task for exactly one tick
  - `yield return state.Sleep(long)` to suspend your task for the specified number of ticks
  - `yield return state.Sleep(TimeSpan)` to suspend your task for the specified duration
  - `yield return state.Await(task)` to suspend your task until the specified task has completed
  - `yield return state.Await(task method)` to create and start a task on the scheduler, then suspend the original task until the new task's completion
  - `yield return state.Success(result)` to complete the task successfully and return `result` to any awaiters
* Basic exception handling
  - If an exception occurs within a task, the scheduler will trap it, assign it to the task, and mark the task as failed
  - Tasks awaiting a failed task will have access to its `task.Exception` property
  - Tasks awaiting a failed task can set `task.ExceptionHandled` = true to prevent the scheduler from shutting down the entire chain of tasks
* The ability to retrieve results from completed tasks
  - After awaiting a task, its result will be stored in `state.result`
  - If you have a reference to a completed task, you can access its result in `task.result`

# Behind the Scenes
The scheduler uses three pairing heaps to keep track of most tasks.  Pairing heaps have relatively good performance compared to many other heaps, including binary heaps.  Their one big pitfall is allocation; however, this particular pairing heap has been written to make use of an object pool to prevent excessive allocation and GC headaches.  All three pairing heaps share a single object pool.
* `Active` - The `Active` pairing heap stores tasks that are active and ready to run, sorted by `PriorityOrdinal`.
  - `PriorityOrdinal` is based on a combination of the tick on which the task was scheduled and the task's priority.  Higher priority tasks will have a PriorityOrdinal closer to the tick in which they were scheduled.  The end result is that all tasks will be run regularly regardless of priority, but higher priority tasks will be run more often.  The goal was that each priority level should roughly double the number of executions allowed to a task relative to lower priority tasks.  That part still needs work.  The scheduler currently places a little too much emphasis on higher priority tasks, but the general idea that everything runs still holds true.
* `PendingTime` - The `PendingTime` pairing heap stores tasks that are scheduled to start at a specific time in the future.  After that time arrives, the task is moved into the `Active` queue.
* `PendingTick` - The `PendingTick` pairing heap stores tasks that are scheduled to execute in a specific future tick.  After that tick arrives, the task is moved to the `Active` queue.

The scheduler itself advances enumerators for tasks within the Active heap as long as capacity allows it, interpeting signals you send via `yield return` to determine how to proceed.
