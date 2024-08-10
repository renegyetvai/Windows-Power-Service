using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
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
        private ServiceConfig _config;

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
            LoadConfig();

            // Check if the power plans are set in the configuration or empty. If empty, stop the service.
            if (string.IsNullOrEmpty(_config.PerformancePowerPlan) || string.IsNullOrEmpty(_config.LowPowerPlan))
            {
                eventLog.WriteEntry("PowerService stopped because the power plans are not set in the configuration.");
                Stop();
            }

            eventLog.WriteEntry("PowerService started.");

            // Initialize the coreCounters list depending on the number of cores.
            int cpuCores = dataProvider.getCoreCount();
            for (int i = 0; i < cpuCores; i++)
            {
                coreCounters.Add(new Queue<float>(_config.QueueSize));
            }

            eventLog.WriteEntry($"Number of CPU cores detected by Power Service: {cpuCores}");

            // Set up a timer that triggers every minute.
            Timer timer = new Timer();
            timer.Interval = _config.MeasurementInterval;
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
                    if (coreCounters[i].Count >= _config.QueueSize)
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
                    if (average >= _config.SingleCoreThreshold)
                    {
                        singleAboveThreshold = true;
                    }

                    totalAverage += average;
                }
                totalAverage /= dataProvider.getCoreCount();

                // Check if the total average CPU usage is above the threshold.
                if (totalAverage >= _config.MultiCoreThreshold)
                {
                    totalAboveThreshold = true;
                }

                // Decide which power plan to trigger based on the results.
                if (totalAboveThreshold || singleAboveThreshold)
                {
                    // Trigger the performance mode.
                    TriggerEnergyProfile(_config.PerformancePowerPlan);
                }
                else
                {
                    // Trigger the power saving mode.
                    TriggerEnergyProfile(_config.LowPowerPlan);
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

        private void LoadConfig()
        {
            string rootFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string configFolder = Path.Combine(rootFolder, "PowerService");
            string configFile = Path.Combine(configFolder, "config.json");

            if (!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
            }

            if (!File.Exists(configFile))
            {
                _config = new ServiceConfig
                {
                    MeasurementDuration = 300,
                    MeasurementInterval = 5000,
                    QueueSize = 60,
                    SingleCoreThreshold = 20,
                    MultiCoreThreshold = 20,
                    PerformancePowerPlan = "",
                    LowPowerPlan = ""
                };

                string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configFile, json);
            }
            else
            {
                string json = File.ReadAllText(configFile);
                _config = JsonSerializer.Deserialize<ServiceConfig>(json);
            }
        }

    }
}
