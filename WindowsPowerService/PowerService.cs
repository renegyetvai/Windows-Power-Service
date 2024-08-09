using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WindowsPowerService
{
    public partial class PowerService : ServiceBase
    {
        // Constant values
        private const int MEASUREMENT_DURATION = 300; // 5 minutes in seconds
        private const int MEASUREMENT_INTERVAL = 5000; // 5 seconds in milliseconds
        private const int QUEUE_SIZE = MEASUREMENT_DURATION / (MEASUREMENT_INTERVAL / 1000);
        private const float SINGLE_CORE_THRESHOLD = 20; // 20% CPU usage
        private const float MULTI_CORE_THRESHOLD = 20; // 20% CPU usage
        private const string PERFORMANCE_POWER_PLAN = "381b4222-f694-41f0-9685-ff5bb260df2e";
        private const string LOW_POWER_PLAN = "a1841308-3541-4fab-bc81-f71556f20b4a";

        // Create a new DataProvider object.
        private DataProvider dataProvider = new DataProvider();
        // Set up data structures to store the data.
        private List<Queue<float>> coreCounters;

        public PowerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Initialize the coreCounters list depending on the number of cores.
            int cpuCores = dataProvider.getCoreCount();
            for (int i = 0; i < cpuCores; i++)
            {
                coreCounters.Add(new Queue<float>(QUEUE_SIZE));
            }

            // Set up a timer that triggers every minute.
            Timer timer = new Timer();
            timer.Interval = MEASUREMENT_INTERVAL;
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        protected override void OnStop()
        {
        }

        private void OnTimer(object sender, ElapsedEventArgs args)
        {
            // Get the CPU usage per core and store each value in the corresponding queue.
            float[] usages = dataProvider.GetCpuUsagePerCore();
            for (int i = 0; i < usages.Length; i++)
            {
                if (coreCounters[i].Count >= QUEUE_SIZE)
                {
                    coreCounters[i].Dequeue();
                }
                coreCounters[i].Enqueue(usages[i]);
            }

            // Calculate the average CPU usage per core and all cores.
            float totalAverage = 0;
            for (int i = 0; i < coreCounters.Count; i++)
            {
                float sum = 0;
                foreach (float usage in coreCounters[i])
                {
                    sum += usage;
                }
                float average = sum / coreCounters[i].Count;

                // Check if the average CPU usage is above the threshold.
                if (average >= SINGLE_CORE_THRESHOLD)
                {
                    // Trigger the performance mode.
                    TriggerEnergyProfile(PERFORMANCE_POWER_PLAN);
                } else if (average < SINGLE_CORE_THRESHOLD)
                {
                    // Trigger the power saving mode.
                    TriggerEnergyProfile(LOW_POWER_PLAN);
                }

                totalAverage += average;
            }
            totalAverage /= dataProvider.getCoreCount();

            // Check if the total average CPU usage is above the threshold.
            if (totalAverage >= MULTI_CORE_THRESHOLD)
            {
                // Trigger the performance mode.
                TriggerEnergyProfile(PERFORMANCE_POWER_PLAN);
            } else if (totalAverage < MULTI_CORE_THRESHOLD)
            {
                // Trigger the power saving mode.
                TriggerEnergyProfile(LOW_POWER_PLAN);
            }
        }

        private void TriggerEnergyProfile(string powerPlanGuid)
        {
            // Construct the powercfg command
            string command = $"powercfg /setactive {powerPlanGuid}";

            // Execute the command
            ExecuteCommand(command);
        }

        static void ExecuteCommand(string command)
        {
            try
            {
                // Initialize a new process to run the command
                ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd", "/c " + command);
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;

                Process process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();

                // Read the output of the command
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

    }
}
