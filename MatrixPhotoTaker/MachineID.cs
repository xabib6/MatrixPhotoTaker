using Microsoft.Win32;
using System.Windows;

public static class MachineID
{
    static string[] Hierarchy = { "SOFTWARE", "Antilatency", "Factory"};
    static string Key = "MachineID";
    public static string? GetId()
    {
        RegistryKey key = Registry.CurrentUser;

        foreach (var path in Hierarchy)
        {
            key = key.CreateSubKey(path);
        }

        var values = key.GetValueNames();

        foreach (var value in values)
        {
            if (value.StartsWith(Key))
            {
                return key.GetValue(value)?.ToString();
            }
        }
        return null;
    }    
    
}
