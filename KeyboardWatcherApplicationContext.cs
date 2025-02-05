namespace ProArtKeyboardWatcher;

public class KeyboardWatcherApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly HiddenForm _hiddenForm;

    public KeyboardWatcherApplicationContext()
    {
        // Initialize the hidden window
        _hiddenForm = new HiddenForm();

        // Create a context menu for the tray icon
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Exit", null, Exit!);

        // Initialize the tray icon
        _trayIcon = new NotifyIcon()
        {
            Icon = SystemIcons.GetStockIcon(StockIconId.DriveRam, StockIconOptions.ShellIconSize),
            ContextMenuStrip = contextMenu,
            Visible = true,
            Text = "ProArt Keyboard Watcher",
        };

        // Register for device notifications
        _hiddenForm.RegisterForDeviceNotifications();
    }

    // Exit event handler for the tray icon
    private void Exit(object sender, EventArgs e)
    {
        // Hide tray icon before exit
        _trayIcon.Visible = false;

        // Close the hidden form
        _hiddenForm.Close();

        // Exit the application
        Application.Exit();
    }
}