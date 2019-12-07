using System.Management;

namespace MobileAppServer.ServerObjects
{
    public class CPUID
    {
        public static string GetCPUIdentifier()
        {
            string key = "cpuId";
            var cached = CacheRepository<string>.Get(key);
            if (cached != null)
                return cached.Value;
            try
            {
                string cpuInfo = "";
                ManagementClass managClass = new ManagementClass("win32_processor");
                ManagementObjectCollection managCollec = managClass.GetInstances();

                foreach (ManagementObject managObj in managCollec)
                {
                    cpuInfo = (string)managObj.Properties["processorID"].Value;
                    break;
                }

                CacheRepository<string>.Set(key, cpuInfo, 60);
                return cpuInfo;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
