using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

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
        private List<Queue<float>> coreCounters = new List<Queue<float>>();
        // Eventlog to log the service's activities.
        private EventLog eventLog = new EventLog();

        public PowerService()
        {
            InitializeComponent();

            // Setup the event log.
            if (!EventLog.SourceExists("PowerServiceSource"))
            {
                EventLog.CreateEventSource("PowerServiceSource", "PowerServiceLog");
            }
            eventLog.Source = "PowerServiceSource";
            eventLog.Log = "PowerServiceLog";
        }

        protected override void OnStart(string[] args)
        {
            Thread.Sleep(3000);
            eventLog.WriteEntry("PowerService started.");

            // Initialize the coreCounters list depending on the number of cores.
            int cpuCores = dataProvider.getCoreCount();
            for (int i = 0; i < cpuCores; i++)
            {
                coreCounters.Add(new Queue<float>(QUEUE_SIZE));
            }

            eventLog.WriteEntry($"Number of CPU cores detected by Power Service: {cpuCores}");

            // Set up a timer that triggers every minute.
            Timer timer = new Timer();
            timer.Interval = MEASUREMENT_INTERVAL;
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();

            eventLog.WriteEntry("PowerService timer started.");
        }

        protected override void OnStop()
        {
            eventLog.WriteEntry("PowerService stopped.");
        }

        private void OnTimer(object sender, ElapsedEventArgs args)
        {
            try
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
                bool totalAboveThreshold = false;
                bool singleAboveThreshold = false;
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
                        singleAboveThreshold = true;
                    }

                    totalAverage += average;
                }
                totalAverage /= dataProvider.getCoreCount();

                // Check if the total average CPU usage is above the threshold.
                if (totalAverage >= MULTI_CORE_THRESHOLD)
                {
                    totalAboveThreshold = true;
                }

                // Decide which power plan to trigger based on the results.
                if (totalAboveThreshold || singleAboveThreshold)
                {
                    // Trigger the performance mode.
                    TriggerEnergyProfile(PERFORMANCE_POWER_PLAN);
                }
                else
                {
                    // Trigger the power saving mode.
                    TriggerEnergyProfile(LOW_POWER_PLAN);
                }
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry($"Error in OnTimer: {ex.Message}");
            }
        }

        private void TriggerEnergyProfile(string powerPlanGuid)
        {
            // Construct the powercfg command
            string command = $"powercfg /setactive {powerPlanGuid}";

            // Execute the command
            ExecuteCommand(command);
        }

        private void ExecuteCommand(string command)
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

                if (process.ExitCode != 0)
                {
                    eventLog.WriteEntry($"Bad error code from cmd command: {process.ExitCode}\n" +
                        $"Contents of stdout: {process.StandardOutput}\n" +
                        $"Contents of stderr: {process.StandardError}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                eventLog.WriteEntry($"Error while executing command in cmd: {ex.Message}");
            }
        }

    }
}
