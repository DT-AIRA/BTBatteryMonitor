namespace BTBatteryMonitor.Models
{
    public class DeviceBatteryItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DeviceTypeIcon { get; set; } = "\uE702"; // デフォルト: Bluetoothアイコン
        public int BatteryLevel { get; set; }

        public string BatteryText => $"{BatteryLevel}%";
        public bool IsLowBattery => BatteryLevel <= 20;

        // 電池内部ゲージの幅 (電池内部の最大幅を14pxとした場合の比例幅)
        public double FillWidth => Math.Max(2, Math.Min(14, 14.0 * (BatteryLevel / 100.0)));

        // ゲージの色 (20%以下ならソフトレッド、それ以外はソフトグリーン)
        public string LevelBrushColor => IsLowBattery ? "#FF5555" : "#4CD964";
    }
}
