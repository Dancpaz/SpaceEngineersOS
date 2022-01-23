using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{ 
    class ScheduleTaskActiveComparer : IComparer<IScheduleTask>
    {
        public int Compare(IScheduleTask a, IScheduleTask b) => a.PriorityOrdinal.CompareTo(b.PriorityOrdinal);
        //{
        //    if (a == null)
        //        return b == null ? 0 : -1;
        //    if (b == null)
        //        return 1;

        //    var result = a.Priority.CompareTo(b.Priority);
        //    if (result != 0)
        //        return result;

        //    result = a.PriorityOrdinal.CompareTo(b.PriorityOrdinal);
        //    if (result != 0)
        //        return result;

        //    return result;
        //}
    }
}
