using System.Diagnostics; // Explicitly include System.Diagnostics.Debug
using Android.App; // Add this import for the correct IntentFilter attribute
using Android.Content;
using Android.Content.PM;
using Android.OS;
using BatteryMonitorApp.Services;

namespace BatteryMonitorApp.Platforms.Android
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionBatteryChanged })] // Correct namespace for IntentFilter
    public class BatteryPluggedReceiver : BroadcastReceiver
    {
        private NotificationService? _notificationService;

        // Default constructor required for system instantiation
        public BatteryPluggedReceiver() { }

        // This constructor is used when registering manually in code
        public BatteryPluggedReceiver(NotificationService service)
        {
            _notificationService = service;
        }

        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent == null) return;

            if (intent.Action == Intent.ActionBatteryChanged)
            {
                int status = intent.GetIntExtra(BatteryManager.ExtraStatus, (int)BatteryStatus.Unknown);
                bool isCharging = status == (int)BatteryStatus.Charging ||
                                  status == (int)BatteryStatus.Full;

                int plugged = intent.GetIntExtra(BatteryManager.ExtraPlugged, -1);
                bool usbCharge = plugged == (int)BatteryPlugged.Usb;
                bool acCharge = plugged == (int)BatteryPlugged.Ac;
                bool wirelessCharge = plugged == (int)BatteryPlugged.Wireless;
                bool isPluggedIn = usbCharge || acCharge || wirelessCharge;

                System.Diagnostics.Debug.WriteLine($"BatteryPluggedReceiver: isCharging={isCharging}, isPluggedIn={isPluggedIn}");

                // Only call DismissLowBatteryNotification if _notificationService is set
                if (isPluggedIn && _notificationService != null)
                {
                    _notificationService.DismissLowBatteryNotification();
                }
            }
        }
    }

    public enum BatteryPlugged
    {
        Usb = 1,
        Ac = 2,
        Wireless = 4
    }
}