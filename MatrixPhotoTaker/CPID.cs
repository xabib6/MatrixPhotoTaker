using System;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

public static class PCID
{

    public static string GetId()
    {
        string cpuInfo = string.Empty;
        ManagementClass mc = new ManagementClass("win32_processor");
        ManagementObjectCollection moc = mc.GetInstances();

        foreach (ManagementObject mo in moc)
        {
            cpuInfo = mo.Properties["processorID"].Value.ToString();
            break;
        }
        return cpuInfo;
    }

    
    

    
}
