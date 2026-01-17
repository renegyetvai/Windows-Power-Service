namespace WindowsPowerService
{
    internal class ServiceConfig
    {
        public int MeasurementDuration { get; set; }
        public int MeasurementInterval { get; set; }
        public int QueueSize { get; set; }
        public float SingleCoreThreshold { get; set; }
        public float MultiCoreThreshold { get; set; }
        public string PerformancePowerPlan { get; set; }
        public string LowPowerPlan { get; set; }
    }
}
