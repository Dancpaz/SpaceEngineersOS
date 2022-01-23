using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    public class AverageCalculator
    {
        /// <summary>
        /// Determines how meaningful each tick is.
        /// </summary>
        /// <remarks>
        /// Lower makes it more accurate, higher makes it more responsive.  Generally, you want this to be higher than 
        /// the equivalent setting in the server you are playing on.  This will give the scheduler a chance to see
        /// and correct performance problems so the plugin does not have to intervene.
        /// 
        /// The default setting in the PBLimiter Torch plugin is 0.01, so we will use 0.05.  That said, there is room
        /// to tweak this value to find a sweet spot depending on how close to the limit you are willing to come.
        /// </remarks>
        double TickSignificance;
        public double Value { get; private set; }

        public AverageCalculator(double frameSignificance)
        {
            TickSignificance = frameSignificance;
        }

        public double Next(long ticks, double value)
        {
            return (Value =                                     // set and return both
                Value * Math.Pow(1 - TickSignificance, ticks)   // reduce impact of previous ticks based on elapsed time
                + value * TickSignificance);                    // add current tick value
        }
    }

}
