namespace SipaaKernel.Core
{
    public class ProcessManager
    {
        public static List<Process> Processes { get; set; }

        static Logger logger;

        public static void Init()
        {
            Processes = new List<Process>();
            logger = new();
            logger.LoggerSource = "SK:ProcessManager";
        }

        public static void Yield()
        {
            try
            {
                foreach (Process process in Processes)
                {
                    process.Update();
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.SKBugCheck(ex);
            }
        }

        public static bool StartProcess(Process process)
        {
            logger.Log($"Starting '{process.Name}' process.", Logger.LogType.Info);

            Processes.Add(process);

            var r = process.Start();

            if (r)
                logger.Log($"'{process.Name}' process has been started.", Logger.LogType.Success);
            else { logger.Log($"'{process.Name}' process has encountered an error while starting.", Logger.LogType.Error); }

            return r;
        }

        public static bool StopProcess(Process process)
        {
            logger.Log($"Stopping '{process.Name}' process.", Logger.LogType.Info);

            Processes.Remove(process);

            var r = process.Stop();

            if (r)
                logger.Log($"'{process.Name}' process has been stppped.", Logger.LogType.Success);
            else { logger.Log($"'{process.Name}' process has encountered an error while stopping.", Logger.LogType.Error); }

            return r;
        }

        public static Process GetProcessByName(string name)
        {
            foreach (Process process in Processes)
            {
                if (process.Name == name)
                {
                    return process;
                }
            }

            return null;
        }
    }
}