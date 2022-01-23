using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    class ScheduleTaskTickComparer : IComparer<IScheduleTask>
    {
        public int Compare(IScheduleTask a, IScheduleTask b) => a.TargetTick.CompareTo(b.TargetTick);
    }
}
