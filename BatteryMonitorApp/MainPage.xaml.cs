using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using System.Diagnostics; // Ensure this is present
#if ANDROID
using Android.Content;
using static Microsoft.Maui.ApplicationModel.Platform; // Use 'using static' for accessing Platform.CurrentActivity
#endif
using BatteryMonitorApp.Services;

namespace BatteryMonitorApp
{
    public partial class MainPage : ContentPage
    {
        private const string BatteryThresholdKey = "BatteryThreshold";
        private const string NotificationTitleKey = "NotificationTitle";
        private const string NotificationMessageKey = "NotificationMessage";

        private NotificationService _notificationService;
        private bool _isBatteryLowNotificationActive = false;

        public MainPage()
        {
            InitializeComponent();
            LoadSettings();

#if ANDROID
            if (CurrentActivity is Android.App.Activity androidActivity)
            {
                _notificationService = new NotificationService(androidActivity);
            }
            else
            {
                _notificationService = new NotificationService(Android.App.Application.Context);
                System.Diagnostics.Debug.WriteLine("Warning: Platform.CurrentActivity was null, falling back to Application.Context");
            }
#else
            System.Diagnostics.Debug.WriteLine("Warning: CurrentActivity is only available on Android.");
#endif

            BatteryThresholdSlider.ValueChanged += OnBatteryThresholdSliderValueChanged;
            Battery.BatteryInfoChanged += OnBatteryInfoChanged;
            CheckBatteryAndNotify();
        }

        private void OnBatteryThresholdSliderValueChanged(object? sender, ValueChangedEventArgs e)
        {
            BatteryThresholdLabel.Text = $"Current Threshold: {e.NewValue:F0}%";
        }

        private void OnSaveSettingsClicked(object? sender, EventArgs e)
        {
            Preferences.Set(BatteryThresholdKey, (int)BatteryThresholdSlider.Value);
            Preferences.Set(NotificationTitleKey, NotificationTitleEntry.Text);
            Preferences.Set(NotificationMessageKey, NotificationMessageEntry.Text);

            DisplayAlert("Settings Saved", "Your battery monitoring settings have been saved.", "OK");
            CheckBatteryAndNotify();
        }

        private void OnTestNotificationClicked(object? sender, EventArgs e)
        {
            _notificationService.ShowPersistentLowBatteryNotification(
                NotificationTitleEntry.Text + " (TEST)",
                NotificationMessageEntry.Text + " (This is a test notification. Plug in device to dismiss.)"
            );
            _isBatteryLowNotificationActive = true;
        }

        private void OnBatteryInfoChanged(object? sender, BatteryInfoChangedEventArgs e)
        {
            // FIX: Explicitly use System.Diagnostics.Debug
            System.Diagnostics.Debug.WriteLine($"BatteryInfoChanged: ChargeLevel={e.ChargeLevel}, State={e.State}, PowerSource={e.PowerSource}");
            CheckBatteryAndNotify();
        }

        private void CheckBatteryAndNotify()
        {
            int threshold = Preferences.Get(BatteryThresholdKey, 20);
            string title = Preferences.Get(NotificationTitleKey, "Low Battery Warning!");
            string message = Preferences.Get(NotificationMessageKey, "Your battery is low. Please plug in your phone.");

            var battery = Battery.Default;
            double currentBatteryLevel = battery.ChargeLevel * 100;

            // FIX: Explicitly use System.Diagnostics.Debug
            System.Diagnostics.Debug.WriteLine($"Current Battery: {currentBatteryLevel:F0}% Threshold: {threshold}%");

            if (currentBatteryLevel <= threshold && battery.State != BatteryState.Charging && battery.State != BatteryState.Full)
            {
                if (!_isBatteryLowNotificationActive)
                {
                    _notificationService.ShowPersistentLowBatteryNotification(title, message);
                    _isBatteryLowNotificationActive = true;
                }
            }
            else if (currentBatteryLevel > threshold || battery.State == BatteryState.Charging || battery.State == BatteryState.Full)
            {
                if (_isBatteryLowNotificationActive)
                {
                    _notificationService.DismissLowBatteryNotification();
                    _isBatteryLowNotificationActive = false;
                }
            }
        }

        private void LoadSettings()
        {
            int savedThreshold = Preferences.Get(BatteryThresholdKey, 20);
            string savedTitle = Preferences.Get(NotificationTitleKey, "Low Battery Warning!");
            string savedMessage = Preferences.Get(NotificationMessageKey, "Your battery is low. Please plug in your phone.");

            BatteryThresholdSlider.Value = savedThreshold;
            BatteryThresholdLabel.Text = $"Current Threshold: {savedThreshold}%";
            NotificationTitleEntry.Text = savedTitle;
            NotificationMessageEntry.Text = savedMessage;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Battery.BatteryInfoChanged -= OnBatteryInfoChanged;
            _notificationService?.DismissLowBatteryNotification();
        }
    }
}