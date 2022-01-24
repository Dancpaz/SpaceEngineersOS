using System;
using System.Collections.Generic;
using System.Text;

//using static IngameScript.Program;

namespace IngameScript
{
    public delegate IEnumerable<ITaskYield> TaskCreator(ITaskState state);
    public delegate IEnumerable<TaskYield<T>> TaskCreator<T>(TaskState<T> state);
    public delegate IEnumerable<TaskYield<TResult>> TaskCreator<TArg, TResult>(TaskState<TResult> state, TArg arg);
}
