using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    class ScheduleTaskTimeComparer : IComparer<IScheduleTask>
    {
        public int Compare(IScheduleTask a, IScheduleTask b) => a.TargetTime.CompareTo(b.TargetTime);
    }
}
