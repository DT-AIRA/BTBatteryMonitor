using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Win32;
using BTBatteryMonitor.ViewModels;

namespace BTBatteryMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WM_DEVICECHANGE = 0x0219;
        private const string StartupRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppRegistryName = "BTBatteryMonitor";
        private readonly DispatcherTimer _debounceTimer;

        public MainWindow()
        {
            InitializeComponent();

            // デバイス接続・切断イベントが短時間に連続発生した場合のデバウンスタイマー (700ms)
            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(700)
            };
            _debounceTimer.Tick += async (s, e) =>
            {
                _debounceTimer.Stop();
                if (DataContext is MainViewModel vm)
                {
                    await vm.RefreshAsync();
                }
            };
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // スタートアップ登録状態の初期判定
            UpdateStartupMenuState();

            // Windows PnPメッセージ (WM_DEVICECHANGE) のフック登録
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // OSからBluetooth機器等の接続・切断通知を受信（オブザーバー・パターン）
            if (msg == WM_DEVICECHANGE)
            {
                _debounceTimer.Stop();
                _debounceTimer.Start();
            }

            return IntPtr.Zero;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private async void MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.RefreshAsync();
            }
        }

        private void MenuItem_AlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
            if (sender is MenuItem item)
            {
                item.IsChecked = Topmost;
            }
        }

        private void MenuItem_LaunchAtStartup_Click(object sender, RoutedEventArgs e)
        {
            bool enable = MenuLaunchAtStartup.IsChecked;
            SetStartupRegistry(enable);
        }

        private void UpdateStartupMenuState()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, false);
                var val = key?.GetValue(AppRegistryName);
                MenuLaunchAtStartup.IsChecked = val != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Failed to read startup registry: {ex.Message}");
            }
        }

        private void SetStartupRegistry(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);
                if (key == null) return;

                if (enable)
                {
                    string exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName ?? "";
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        key.SetValue(AppRegistryName, $"\"{exePath}\"");
                    }
                }
                else
                {
                    key.DeleteValue(AppRegistryName, false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Failed to update startup registry: {ex.Message}");
                // 失敗した場合はメニュー表示を元の状態に戻す
                UpdateStartupMenuState();
            }
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}