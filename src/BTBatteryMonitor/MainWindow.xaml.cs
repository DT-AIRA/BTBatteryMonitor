using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using BTBatteryMonitor.ViewModels;

namespace BTBatteryMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WM_DEVICECHANGE = 0x0219;
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

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}