using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using BTBatteryMonitor.Models;
using BTBatteryMonitor.Services;

namespace BTBatteryMonitor.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly BluetoothBatteryService _batteryService;
        private readonly DispatcherTimer _timer;
        private ObservableCollection<DeviceBatteryItem> _devices = new();
        private bool _isLoading = true;
        private string _statusMessage = "Searching devices...";

        public ObservableCollection<DeviceBatteryItem> Devices
        {
            get => _devices;
            set
            {
                _devices = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasDevices));
            }
        }

        public bool HasDevices => Devices.Count > 0;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            _batteryService = new BluetoothBatteryService();

            // イベント検知のバックアップ用ポーリングタイマー (60秒間隔)
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(60)
            };
            _timer.Tick += async (s, e) => await RefreshAsync();

            // 初期読み込み開始
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await RefreshAsync();
            _timer.Start();
        }

        /// <summary>
        /// デバイス一覧を手動または定期更新する
        /// </summary>
        public async Task RefreshAsync()
        {
            try
            {
                IsLoading = true;
                var latestDevices = await _batteryService.GetConnectedDevicesAsync();

                // UIスレッドでコレクションを更新
                Devices.Clear();
                foreach (var dev in latestDevices)
                {
                    Devices.Add(dev);
                }

                OnPropertyChanged(nameof(HasDevices));

                if (Devices.Count == 0)
                {
                    StatusMessage = "No devices found";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Error scanning devices";
                System.Diagnostics.Debug.WriteLine($"[MainViewModel] Refresh error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
