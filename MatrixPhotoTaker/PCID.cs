using Microsoft.Win32;
using System.Windows;

public static class PCID
{
    static string[] Hierarchy = { "Software","Antilatency", "Factory", "MachineId", "ID" };
    public static string GetId()
    {
        RegistryKey key = Registry.CurrentUser;
        for (int i = 0; i < Hierarchy.Length-1; i++)
        {
            key = key.CreateSubKey(Hierarchy[i]);
        }

        string s = null;        
        if (key != null)
        {
            s = key.GetValue(Hierarchy[Hierarchy.Length-1])?.ToString();
            key.Close();            
        }
        return s;
    }

    public static void WriteInRegister(string PCID)
    {

        RegistryKey key = Registry.CurrentUser;
        for (int i = 0; i < Hierarchy.Length - 1; i++)
        {
            key = key.CreateSubKey(Hierarchy[i]);
        }

        key.SetValue(Hierarchy[Hierarchy.Length-1], PCID);
        key.Close();
    }
    
}
