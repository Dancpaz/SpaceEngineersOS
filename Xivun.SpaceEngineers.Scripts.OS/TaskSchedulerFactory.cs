using System;
using System.Collections.Generic;
using System.Text;

using IngameScript.Containers;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public class TaskSchedulerFactory
    {
        public TaskScheduler Create(MyGridProgram program, TimeSpan maxUpdateTime, double maxLoadPercent)
        {
            var shortAverage = new AverageCalculator(0.025);
            var longAverage = new AverageCalculator(0.01);

            var capacity = new CapacityManager(program, shortAverage, longAverage, maxUpdateTime.TotalMilliseconds, 30000);

            var heapFactory = new PairingHeapFactory();

            // share a single node pool between all task heaps
            var pool = heapFactory.CreatePool<IScheduleTask>();

            var scheduler = new TaskScheduler(comparer => heapFactory.Create(comparer, pool), capacity);

            return scheduler;
        }
    }
}
