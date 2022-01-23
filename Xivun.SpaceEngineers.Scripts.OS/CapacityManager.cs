using System;
using System.Collections.Generic;
using System.Text;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public class CapacityManager
    {
        IMyGridProgramRuntimeInfo Runtime;
        int AbsoluteMaxInstructionCount;
        AverageCalculator ShortAverage;
        AverageCalculator LongAverage;
        int TicksPerUpdate;
        double AverageMsMax;
        int MaxInstructionCount;

        public TimeSpan UpdateTime { get; private set; }
        public double LongAverageRuntimeMs => LongAverage.Value;
        public double ShortAverageRuntimeMs => ShortAverage.Value;

        


        public CapacityManager(MyGridProgram program, AverageCalculator shortAverage, AverageCalculator longAverage, double averageMsMax, int? absoluteMaxInstructionCount = null)
        {
            Runtime = program.Runtime;
            AbsoluteMaxInstructionCount = absoluteMaxInstructionCount ?? Runtime.MaxInstructionCount;
            ShortAverage = shortAverage;
            LongAverage = longAverage;
            AverageMsMax = averageMsMax;
            TicksPerUpdate = GetTicksPerUpdate();
        }

        public bool HasCapacity()
        {
            return Runtime.CurrentInstructionCount <= MaxInstructionCount;
        }

        public void Update(TimeSpan currentTime)
        {
            UpdateTime = currentTime;
            MaxInstructionCount = GetMaxInstructionCount();
        }

        int GetMaxInstructionCount()
        {
            var averageMs = Math.Max(
                ShortAverage.Next(TicksPerUpdate, Runtime.LastRunTimeMs),
                LongAverage.Next(TicksPerUpdate, Runtime.LastRunTimeMs));

            var pctCapacity = Math.Max(AverageMsMax - averageMs, 0) / AverageMsMax;
            pctCapacity = Curve3(pctCapacity);

            return (int)(AbsoluteMaxInstructionCount * pctCapacity);
        }

        double Curve0(double pct) => pct;
        double Curve1(double pct) => Math.Sin(pct * Math.PI * 0.5);
        double Curve2(double pct) => Math.Pow(pct, 2.0);
        double Curve3(double pct) => Math.Pow(pct, 0.5);
        

        

        int GetTicksPerUpdate()
        {
            switch(Runtime.UpdateFrequency)
            {
                case UpdateFrequency.Update1: return 1;
                case UpdateFrequency.Update10: return 10;
                case UpdateFrequency.Update100: return 100;
                default: throw new InvalidOperationException($"OS requires the use of Runtime.UpdateFrequency.");
            }
        }
    }
}
