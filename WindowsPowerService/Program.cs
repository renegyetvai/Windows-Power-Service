using System.ServiceProcess;

namespace WindowsPowerService
{
    internal static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new PowerService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
