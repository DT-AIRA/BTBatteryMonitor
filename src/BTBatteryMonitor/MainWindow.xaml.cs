using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Win32;
using BTBatteryMonitor.Services;
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
        private static string StartupShortcutPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "BTBatteryMonitor.lnk");

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

            // 前回のウィンドウ位置を復元
            RestoreWindowPosition();

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
                // ドラッグ移動完了時に位置を自動保存
                SaveWindowPosition();
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
            SetStartupShortcut(enable);
        }

        private void UpdateStartupMenuState()
        {
            try
            {
                // 旧レジストリ方式の登録が残っている場合はクリーンアップ
                CleanupLegacyRegistry();

                MenuLaunchAtStartup.IsChecked = File.Exists(StartupShortcutPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Failed to check startup shortcut: {ex.Message}");
            }
        }

        private void SetStartupShortcut(bool enable)
        {
            try
            {
                string shortcutPath = StartupShortcutPath;

                if (enable)
                {
                    string exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName ?? "";
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                        if (shellType != null)
                        {
                            dynamic? shell = Activator.CreateInstance(shellType);
                            if (shell != null)
                            {
                                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                                shortcut.TargetPath = exePath;
                                shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                                shortcut.Description = "Bluetooth Battery Monitor";
                                shortcut.Save();
                            }
                        }
                    }
                }
                else
                {
                    if (File.Exists(shortcutPath))
                    {
                        File.Delete(shortcutPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Failed to update startup shortcut: {ex.Message}");
                UpdateStartupMenuState();
            }
        }

        private static void CleanupLegacyRegistry()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);
                if (key?.GetValue(AppRegistryName) != null)
                {
                    key.DeleteValue(AppRegistryName, false);
                }
            }
            catch { }
        }

        private void RestoreWindowPosition()
        {
            try
            {
                var settings = AppSettingsService.Load();
                if (settings.WindowLeft.HasValue && settings.WindowTop.HasValue)
                {
                    double left = settings.WindowLeft.Value;
                    double top = settings.WindowTop.Value;

                    // 画面外に出ていないか（仮想スクリーン領域内にあるか）をチェック
                    double virtualLeft = SystemParameters.VirtualScreenLeft;
                    double virtualTop = SystemParameters.VirtualScreenTop;
                    double virtualWidth = SystemParameters.VirtualScreenWidth;
                    double virtualHeight = SystemParameters.VirtualScreenHeight;

                    if (left >= virtualLeft && (left + 50) <= (virtualLeft + virtualWidth) &&
                        top >= virtualTop && (top + 50) <= (virtualTop + virtualHeight))
                    {
                        WindowStartupLocation = WindowStartupLocation.Manual;
                        Left = left;
                        Top = top;
                    }

                }
            }
            catch { }
        }

        private void SaveWindowPosition()
        {
            try
            {
                AppSettingsService.Save(new AppSettings
                {
                    WindowLeft = Left,
                    WindowTop = Top
                });
            }
            catch { }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowPosition();
            base.OnClosing(e);
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}