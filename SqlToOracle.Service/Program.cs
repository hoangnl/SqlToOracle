using System.ServiceProcess;

namespace SqlToOracle.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new SyncData() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
