using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using BTBatteryMonitor.Models;

namespace BTBatteryMonitor.Services
{
    public class BluetoothBatteryService
    {
        #region Win32 SetupAPI P/Invoke (PnPデバイス用)

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVPROPKEY
        {
            public Guid fmtid;
            public uint pid;
        }

        private const uint DIGCF_PRESENT = 0x00000002;
        private const uint DIGCF_ALLCLASSES = 0x00000004;

        // DEVPKEY_Device_BatteryLevel ({104EA319-6EE2-4701-BD47-8DDBF425BBE5}, 2)
        private static readonly DEVPROPKEY PkeyBattery = new DEVPROPKEY
        {
            fmtid = new Guid("104ea319-6ee2-4701-bd47-8ddbf425bbe5"),
            pid = 2
        };

        // DEVPKEY_Device_FriendlyName ({a45c254e-df1c-4efd-8020-67d146a850e0}, 14)
        private static readonly DEVPROPKEY PkeyFriendlyName = new DEVPROPKEY
        {
            fmtid = new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"),
            pid = 14
        };

        // DEVPKEY_NAME ({b725f130-47ef-101a-a5f1-02608c9eebac}, 10)
        private static readonly DEVPROPKEY PkeyDeviceName = new DEVPROPKEY
        {
            fmtid = new Guid("b725f130-47ef-101a-a5f1-02608c9eebac"),
            pid = 10
        };

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(IntPtr ClassGuid, IntPtr Enumerator, IntPtr hwndParent, uint Flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetupDiGetDevicePropertyW(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            ref DEVPROPKEY PropertyKey,
            out uint PropertyType,
            byte[]? PropertyBuffer,
            uint PropertyBufferSize,
            out uint RequiredSize,
            uint Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetupDiGetDeviceInstanceId(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            StringBuilder DeviceInstanceId,
            uint DeviceInstanceIdSize,
            out uint RequiredSize);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        #endregion

        #region WinRT AEP 定数 (BLE用)

        private const string BluetoothProtocolId = "{e0cbf06c-cdb3-4642-bb72-064b60b770b3}";
        private const string PkeyWinRtBatteryLevel = "System.Devices.BatteryLevel";
        private const string PkeyWinRtItemName = "System.ItemNameDisplay";
        private const string PkeyWinRtMajorClass = "System.Devices.Aep.Bluetooth.Cod.MajorDeviceClass";
        private const string PkeyWinRtCategory = "System.Devices.Aep.Category";

        private static readonly string[] AepRequestedProperties = new[]
        {
            PkeyWinRtItemName,
            PkeyWinRtBatteryLevel,
            PkeyWinRtMajorClass,
            PkeyWinRtCategory
        };

        #endregion

        /// <summary>
        /// 現在物理的に接続中(Connected)のBluetooth機器のみを厳密に抽出して一覧を取得
        /// </summary>
        public async Task<List<DeviceBatteryItem>> GetConnectedDevicesAsync()
        {
            var deviceMap = new Dictionary<string, DeviceBatteryItem>(StringComparer.OrdinalIgnoreCase);

            try
            {
                // 1. Win32 SetupAPIによるPnP層スキャン（現在接続中のみフィルタリング）
                await ScanPnpDevicesAsync(deviceMap);

                // 2. WinRTによるAEP層スキャン（BLE接続中デバイス）
                await ScanAepDevicesAsync(deviceMap);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BluetoothBatteryService] Error: {ex.Message}");
            }

            return deviceMap.Values.ToList();
        }

        /// <summary>
        /// SetupAPIを使用してClassic Bluetooth PnPデバイスからバッテリー情報を読み取り、
        /// さらに公式BluetoothConnectionStatusがConnectedの機器のみを登録
        /// </summary>
        private async Task ScanPnpDevicesAsync(Dictionary<string, DeviceBatteryItem> deviceMap)
        {
            IntPtr hDevInfo = SetupDiGetClassDevs(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_ALLCLASSES);
            if (hDevInfo == IntPtr.Zero || hDevInfo == (IntPtr)(-1))
            {
                return;
            }

            // 一旦候補デバイスを収集
            var candidates = new List<(string InstanceId, int BatteryLevel, string RawName)>();

            try
            {
                SP_DEVINFO_DATA devInfoData = new SP_DEVINFO_DATA();
                devInfoData.cbSize = (uint)Marshal.SizeOf(devInfoData);

                uint memberIndex = 0;
                StringBuilder sbId = new StringBuilder(1024);

                while (SetupDiEnumDeviceInfo(hDevInfo, memberIndex, ref devInfoData))
                {
                    memberIndex++;

                    if (!SetupDiGetDeviceInstanceId(hDevInfo, ref devInfoData, sbId, (uint)sbId.Capacity, out _))
                    {
                        continue;
                    }

                    string instanceId = sbId.ToString();

                    // Bluetooth関連デバイスのみ対象
                    if (!instanceId.Contains("BTH", StringComparison.OrdinalIgnoreCase) &&
                        !instanceId.Contains("BLUETOOTH", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    int? batteryLevel = GetPnpBatteryLevel(hDevInfo, ref devInfoData);
                    if (batteryLevel == null)
                    {
                        continue;
                    }

                    string rawName = GetPnpDeviceName(hDevInfo, ref devInfoData);
                    candidates.Add((instanceId, batteryLevel.Value, rawName));
                }
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(hDevInfo);
            }

            // 収集した候補デバイスについて、現在真に接続中(Connected)かを検証
            foreach (var (instanceId, battery, rawName) in candidates)
            {
                string cleanName = CleanDeviceName(rawName);
                if (string.IsNullOrWhiteSpace(cleanName))
                {
                    continue;
                }

                // 既に同名で登録済みなら重複チェックをスキップ
                if (deviceMap.ContainsKey(cleanName))
                {
                    continue;
                }

                // Bluetoothリンクが現在Connectedであるかを厳密検証
                bool isConnected = await CheckIsDeviceConnectedAsync(instanceId);
                if (!isConnected)
                {
                    // 電波が繋がっていない（電源オフ・未接続）デバイスは除外！
                    continue;
                }

                string icon = DetermineAudioOrDeviceIcon(cleanName);

                deviceMap[cleanName] = new DeviceBatteryItem
                {
                    Id = instanceId,
                    Name = cleanName,
                    DeviceTypeIcon = icon,
                    BatteryLevel = Math.Clamp(battery, 0, 100)
                };
            }
        }

        /// <summary>
        /// InstanceIdからBluetooth MACアドレスを抽出し、OSの公式ConnectionStatusがConnectedか判定
        /// </summary>
        private static async Task<bool> CheckIsDeviceConnectedAsync(string instanceId)
        {
            // Bluetooth Base UUID {00000000-0000-1000-8000-00805F9B34FB} 等のGUID部分を除外
            string noGuid = Regex.Replace(instanceId, @"\{[0-9A-Fa-f\-]{36}\}", "");

            // DEV_112233445566 や &112233445566_ などの真のMACアドレス(12桁)を抽出
            var match = Regex.Match(noGuid, @"(?:DEV_|&)([0-9A-Fa-f]{12})(?:_|$|&)");
            if (!match.Success)
            {
                match = Regex.Match(noGuid, @"([0-9A-Fa-f]{12})");
            }

            if (!match.Success)
            {
                return false;
            }

            if (!ulong.TryParse(match.Groups[1].Value, NumberStyles.HexNumber, null, out ulong mac))
            {
                return false;
            }

            try
            {
                // 1. Classic Bluetooth 接続状態チェック
                using var classic = await BluetoothDevice.FromBluetoothAddressAsync(mac);
                if (classic != null && classic.ConnectionStatus == BluetoothConnectionStatus.Connected)
                {
                    return true;
                }
            }
            catch { }

            try
            {
                // 2. BLE 接続状態チェック
                using var ble = await BluetoothLEDevice.FromBluetoothAddressAsync(mac);
                if (ble != null && ble.ConnectionStatus == BluetoothConnectionStatus.Connected)
                {
                    return true;
                }
            }
            catch { }

            return false;
        }

        private static int? GetPnpBatteryLevel(IntPtr hDevInfo, ref SP_DEVINFO_DATA devInfoData)
        {
            DEVPROPKEY key = PkeyBattery;
            byte[] buffer = new byte[32];

            if (SetupDiGetDevicePropertyW(hDevInfo, ref devInfoData, ref key, out _, buffer, (uint)buffer.Length, out uint reqSize, 0))
            {
                if (reqSize >= 1)
                {
                    int val = buffer[0];
                    if (val >= 0 && val <= 100)
                    {
                        return val;
                    }
                }
            }

            return null;
        }

        private static string GetPnpDeviceName(IntPtr hDevInfo, ref SP_DEVINFO_DATA devInfoData)
        {
            string name = GetPnpStringProperty(hDevInfo, ref devInfoData, PkeyFriendlyName);
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            name = GetPnpStringProperty(hDevInfo, ref devInfoData, PkeyDeviceName);
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            return "Bluetooth Device";
        }

        private static string GetPnpStringProperty(IntPtr hDevInfo, ref SP_DEVINFO_DATA devInfoData, DEVPROPKEY key)
        {
            byte[] buffer = new byte[512];
            if (SetupDiGetDevicePropertyW(hDevInfo, ref devInfoData, ref key, out _, buffer, (uint)buffer.Length, out uint reqSize, 0))
            {
                if (reqSize > 2)
                {
                    return Encoding.Unicode.GetString(buffer, 0, (int)reqSize).TrimEnd('\0', ' ');
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// WinRTによるBLEデバイススキャン（IsConnected==trueのみ）
        /// </summary>
        private async Task ScanAepDevicesAsync(Dictionary<string, DeviceBatteryItem> deviceMap)
        {
            try
            {
                string aepAqs = $"System.Devices.Aep.ProtocolId:=\"{BluetoothProtocolId}\" AND System.Devices.Aep.IsConnected:=System.StructuredQueryType.Boolean#True";
                var aepDevices = await DeviceInformation.FindAllAsync(aepAqs, AepRequestedProperties, DeviceInformationKind.AssociationEndpoint);

                foreach (var dev in aepDevices)
                {
                    int? battery = null;
                    if (dev.Properties.TryGetValue(PkeyWinRtBatteryLevel, out var bVal) && bVal != null)
                    {
                        try { battery = Convert.ToInt32(bVal); } catch { }
                    }

                    if (battery == null)
                    {
                        continue;
                    }

                    string rawName = dev.Name;
                    if (string.IsNullOrWhiteSpace(rawName) && dev.Properties.TryGetValue(PkeyWinRtItemName, out var nVal) && nVal is string nStr)
                    {
                        rawName = nStr;
                    }

                    string cleanName = CleanDeviceName(rawName);
                    if (string.IsNullOrWhiteSpace(cleanName))
                    {
                        continue;
                    }

                    if (!deviceMap.ContainsKey(cleanName))
                    {
                        string icon = DetermineDeviceIconFromWinRt(dev.Properties, cleanName);

                        deviceMap[cleanName] = new DeviceBatteryItem
                        {
                            Id = dev.Id,
                            Name = cleanName,
                            DeviceTypeIcon = icon,
                            BatteryLevel = Math.Clamp(battery.Value, 0, 100)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BluetoothBatteryService] AEP Scan error: {ex.Message}");
            }
        }

        private static string CleanDeviceName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            var parenMatch = Regex.Match(name, @"^.*?\((.+?)\)$");
            if (parenMatch.Success)
            {
                name = parenMatch.Groups[1].Value.Trim();
            }

            string cleaned = Regex.Replace(
                name,
                @"\s*(Hands-Free AG|Hands-Free HF Audio|Hands-Free|Avrcp Transport|A2DP SNK|Audio Gateway Service)$",
                "",
                RegexOptions.IgnoreCase
            ).Trim();

            return string.IsNullOrWhiteSpace(cleaned) ? name : cleaned;
        }

        private static string DetermineAudioOrDeviceIcon(string deviceName)
        {
            string lower = deviceName.ToLowerInvariant();
            if (lowerName(lower))
            {
                return "\uE7F6"; // 🎧 Audio
            }
            if (lower.Contains("mouse")) return "\uE962"; // 🖱️ Mouse
            if (lower.Contains("key") || lower.Contains("kbd")) return "\uE92E"; // ⌨️ Keyboard
            if (lower.Contains("game") || lower.Contains("pad") || lower.Contains("xbox")) return "\uE7FC"; // 🎮 Gamepad
            return "\uE7F6";

            static bool lowerName(string l) => l.Contains("head") || l.Contains("ear") || l.Contains("buds") || l.Contains("audio") || l.Contains("sound") || l.Contains("speaker") || l.Contains("airpods");
        }

        private static string DetermineDeviceIconFromWinRt(IReadOnlyDictionary<string, object> properties, string deviceName)
        {
            uint majorClass = 0;
            if (properties.TryGetValue(PkeyWinRtMajorClass, out var mcVal) && mcVal != null)
            {
                try { majorClass = Convert.ToUInt32(mcVal); } catch { }
            }

            string category = string.Empty;
            if (properties.TryGetValue(PkeyWinRtCategory, out var catVal) && catVal is string catStr)
            {
                category = catStr;
            }

            string lowerName = deviceName.ToLowerInvariant();
            string lowerCat = category.ToLowerInvariant();

            if (majorClass == 4 || lowerCat.Contains("audio") || lowerName.Contains("head") || lowerName.Contains("ear") || lowerName.Contains("buds") || lowerName.Contains("audio"))
            {
                return "\uE7F6"; // 🎧 Audio
            }
            if (lowerCat.Contains("mouse") || lowerName.Contains("mouse")) return "\uE962"; // 🖱️ Mouse
            if (lowerCat.Contains("keyboard") || lowerName.Contains("key")) return "\uE92E"; // ⌨️ Keyboard
            if (lowerCat.Contains("game") || lowerName.Contains("game") || lowerName.Contains("xbox")) return "\uE7FC"; // 🎮 Gamepad

            return "\uE702"; // 📶 Bluetooth
        }
    }
}
