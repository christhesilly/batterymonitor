#if ANDROID
using Android.App;
// Ensure the following NuGet package is installed in your project:
// Xamarin.Android.Support.Compat or Xamarin.AndroidX.Core

// Add the necessary using directives for Android namespaces
#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using Android.Media;
using AndroidX.Core.App;
using BatteryMonitorApp.Platforms.Android;
#endif
using Android.Content;
using Android.OS;
using Android.Media;
using AndroidX.Core.App;
using BatteryMonitorApp.Platforms.Android;

// Add this import to resolve 'TypeNotification'
using static Android.Media.RingtoneManager;
using static Android.Media.RingtoneManager; // Fix for CS0138
#endif
using System.Diagnostics; // For System.Diagnostics.Debug

namespace BatteryMonitorApp.Services
{
    public class NotificationService
    {
#if ANDROID
        private const string ChannelId = "BatteryMonitorChannel";
        private const int NotificationId = 1001;
        private NotificationManager? _notificationManager;
        private BatteryPluggedReceiver? _batteryPluggedReceiver;
        private Context _context;

        public NotificationService(Context context)
        {
            _context = context;
            _notificationManager = (NotificationManager?)_context.GetSystemService(Context.NotificationService);
            CreateNotificationChannel();
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O && _notificationManager != null)
            {
                var channelName = "Battery Monitor Notifications";
                var channelDescription = "Notifications for low battery warnings";
                var channel = new NotificationChannel(ChannelId, channelName, NotificationImportance.High)
                {
                    Description = channelDescription
                };

                // Fix the issue with 'TypeNotification'
                var uri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
                var audioAttributes = new AudioAttributes.Builder()
                    .SetContentType(AudioContentType.Sonification)
                    .SetUsage(AudioUsageKind.Notification)
                    .Build();
                channel.SetSound(uri, audioAttributes);

                _notificationManager.CreateNotificationChannel(channel);
            }
        }

        public void ShowPersistentLowBatteryNotification(string title, string message)
        {
            if (_notificationManager == null)
            {
                System.Diagnostics.Debug.WriteLine("NotificationManager is null, cannot show notification.");
                return;
            }

            var intent = _context.PackageManager?.GetLaunchIntentForPackage(_context.PackageName);
            if (intent == null)
            {
                System.Diagnostics.Debug.WriteLine("Launch intent is null, cannot create notification PendingIntent.");
                return;
            }

            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

            var pendingIntent = PendingIntent.GetActivity(
                _context,
                0,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
            );

            var notificationBuilder = new NotificationCompat.Builder(_context, ChannelId)
                .SetSmallIcon(Resource.Mipmap.appicon)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetOngoing(true)
                .SetAutoCancel(false)
                .SetContentIntent(pendingIntent);

            _notificationManager.Notify(NotificationId, notificationBuilder.Build());

            RegisterBatteryPluggedReceiver();
        }

        public void DismissLowBatteryNotification()
        {
            _notificationManager?.Cancel(NotificationId);
            UnregisterBatteryPluggedReceiver();
        }

        private void RegisterBatteryPluggedReceiver()
        {
            if (_batteryPluggedReceiver == null)
            {
                _batteryPluggedReceiver = new BatteryPluggedReceiver(this);
                var intentFilter = new IntentFilter(Intent.ActionBatteryChanged);
                _context.ApplicationContext?.RegisterReceiver(_batteryPluggedReceiver, intentFilter);
            }
        }

        private void UnregisterBatteryPluggedReceiver()
        {
            if (_batteryPluggedReceiver != null)
            {
                _context.ApplicationContext?.UnregisterReceiver(_batteryPluggedReceiver);
                _batteryPluggedReceiver = null;
            }
        }
#else
        // Provide empty implementations for non-Android platforms to avoid CS0246 errors
        public NotificationService(object context) { }
        public void ShowPersistentLowBatteryNotification(string title, string message) { }
        public void DismissLowBatteryNotification() { }
#endif
    }
}