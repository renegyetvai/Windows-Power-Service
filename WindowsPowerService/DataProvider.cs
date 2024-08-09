using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsPowerService
{
    internal class DataProvider
    {
        private List<PerformanceCounter> cpuCounters;
        private int coreCount = -1;

        public DataProvider()
        {
            cpuCounters = new List<PerformanceCounter>();
            coreCount = Environment.ProcessorCount;
            for (int i = 0; i < coreCount; i++)
            {
                cpuCounters.Add(new PerformanceCounter("Processor", "% Processor Time", i.ToString()));
            }
        }

        public float[] GetCpuUsagePerCore()
        {
            float[] usages = new float[cpuCounters.Count];
            for (int i = 0; i < cpuCounters.Count; i++)
            {
                usages[i] = cpuCounters[i].NextValue();
            }
            return usages;
        }

        public int getCoreCount()
        {
            return coreCount;
        }
    }
}
