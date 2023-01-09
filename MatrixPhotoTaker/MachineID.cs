using Microsoft.Win32;
using System;
using System.Text;

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
        var sb = new StringBuilder();

        foreach (var value in values)
        {
            if (value.StartsWith(Key))
            {
                var data = key.GetValue(value) as byte[];
                foreach (var dat in data)
                {
                    sb.Append(Convert.ToChar(dat));
                }
                sb.Remove(sb.Length - 1, 1);
            }
        }
        return sb.ToString();
    }    
    
}
