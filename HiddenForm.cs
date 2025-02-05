using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ProArtKeyboardWatcher;

public class HiddenForm : Form
{
    // Constants for device change notifications
    private const int WmDevicechange = 0x0219;
    private const int DbtDevicearrival = 0x8000; // A device has been inserted
    private const int DbtDeviceremovecomplete = 0x8004; // A device has been removed

    private static readonly Guid KeyboardClassGuid = new Guid("{884b96c3-56ef-11d1-bc8c-00a0c91405dd}");

    public HiddenForm()
    {
        // Hide the form so it doesn't appear to the user
        ShowInTaskbar = false;
        SystemEvents.SessionSwitch += OnSessionSwitch;
        
        Task.Run(async () =>
        {
            await Task.Delay(3000);
            Helper.SetCorrectTableMode();
        });
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason == SessionSwitchReason.SessionUnlock)
        {
            Task.Run(async () =>
            {
                await Task.Delay(3000);
                Helper.SetCorrectTableMode();
            });
        }
    }


    // Override WndProc to handle device change messages
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmDevicechange)
        {
            var lParam = m.LParam;
            if (m.WParam.ToInt32() == DbtDevicearrival || m.WParam.ToInt32() == DbtDeviceremovecomplete)
            {
                var dbh = Marshal.PtrToStructure<DevBroadcastHdr>(lParam);
                if (dbh.dbch_devicetype == DbtDevtypDeviceinterface)
                {
                    var dbdi = Marshal.PtrToStructure<DevBroadcastDeviceinterface>(lParam);
                    if (dbdi.dbcc_classguid == KeyboardClassGuid)
                    {
                        switch (m.WParam.ToInt32())
                        {
                            case DbtDevicearrival:
                                Helper.SetTabletMode(false);
                                break;
                            case DbtDeviceremovecomplete:
                                Helper.SetTabletMode(true);
                                break;
                        }
                    }
                }
            }
        }

        base.WndProc(ref m);
    }

    // Register for device change notifications
    public void RegisterForDeviceNotifications()
    {
        var dbdi = new DevBroadcastDeviceinterface
        {
            dbcc_size = Marshal.SizeOf<DevBroadcastDeviceinterface>(),
            dbcc_devicetype = DbtDevtypDeviceinterface,
            dbcc_reserved = 0,
            dbcc_classguid = KeyboardClassGuid
        };

        RegisterDeviceNotification(
            Handle,
            ref dbdi,
            DeviceNotifyWindowHandle
        );
        // if (notificationHandle == IntPtr.Zero) MessageBox.Show("Failed to register for device notifications.");
    }

    // P/Invoke declarations
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr RegisterDeviceNotification(
        IntPtr hRecipient,
        ref DevBroadcastDeviceinterface notificationFilter,
        int flags
    );

    private const int DbtDevtypDeviceinterface = 0x00000005;
    private const int DeviceNotifyWindowHandle = 0x00000000;

    [StructLayout(LayoutKind.Sequential)]
    private struct DevBroadcastHdr
    {
        public int dbch_size;
        public int dbch_devicetype;
        public int dbch_reserved;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct DevBroadcastDeviceinterface
    {
        public int dbcc_size;
        public int dbcc_devicetype;
        public int dbcc_reserved;
        public Guid dbcc_classguid;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string dbcc_name;
    }
}