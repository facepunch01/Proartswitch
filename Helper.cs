using System.Management;
using Microsoft.Win32;

namespace ProArtKeyboardWatcher;

public abstract class Helper
{
    // Specify the registry key path
    private const string RegistryKeyPath = @"SYSTEM\CurrentControlSet\Control\PriorityControl";

    // Specify the name of the registry value
    private const string ValueName = "ConvertibleSlateMode";

    public static void SetTabletMode(bool tableModeOn)
    {
        using var key = Registry.LocalMachine.OpenSubKey(RegistryKeyPath, writable: true);
        // Set the value
        key?.SetValue(ValueName, tableModeOn ? 0 : 1, RegistryValueKind.DWord);
    }

    public static void SetCorrectTableMode() => SetTabletMode(!IsKeyboardConnected);

    private static bool IsKeyboardConnected
    {
        get
        {
            var targetDeviceId = @"HID\VID_0B05&PID_1B6E&MI_01&COL01\6&1C0661C6&0&0000".ToUpper();
            try
            {
                // Create a searcher for USB controllers
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");
                foreach (var o in searcher.Get())
                {
                    var device = (ManagementObject)o;
                    // Check if the device ID matches
                    if (device["DeviceID"] != null && device["DeviceID"].ToString()?.ToUpper() == targetDeviceId)
                        return true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }
    }
}