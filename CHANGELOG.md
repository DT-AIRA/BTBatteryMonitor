# Changelog / 変更履歴

All notable changes to this project will be documented in this file.
This project adheres to [Semantic Versioning](https://semver.org/).

本プロジェクトのすべての主要な変更は本ファイルに記録されます。
バージョニングは [セマンティック バージョニング](https://semver.org/lang/ja/) に準拠しています。

---

## [v1.1.0] - 2026-09-18

### Added (追加)
- **Widget Color Customization (本体色のカスタマイズ)**:
  - Added "Widget Color" context menu with popular dark presets (Default Dark, Pure Black, Deep Navy, Slate Gray, Deep Wine).
  - Integrated native Win32 Color Palette dialog (`🎨 Color Palette...`) to allow picking any custom color.
  - （右クリックメニューからウィジェット本体の背景色を人気ダークトーン5色およびWindows標準カラーパレットから自由に変更できる機能を追加）
- **Widget Opacity Customization (透明度のカスタマイズ)**:
  - Added "Widget Opacity" context menu with 5 transparency levels (100% Solid, 85% Default, 65% Translucent, 45% Clear, 25% Glass).
  - Maintains crisp readability of text, icons, and battery bars while rendering translucent background card.
  - （文字やアイコンの視認性を保ったまま、本体カードのみの透け具合を5段階から選択できる機能を追加）
- **Appearance Settings Persistence (外観設定の自動保存・復元)**:
  - Automatically persists chosen background color and opacity in `%LOCALAPPDATA%\BTBatteryMonitor\settings.json`.
  - （選択した本体色と透明度を設定ファイルに自動保存し、次回起動時にもそのまま復元）
- **Submenu Support in Modern Context Menu**:
  - Enhanced custom MenuItem template with fade-in Popup and submenu arrow indicators.
  - （右クリックメニューにフェードインアニメーション付きのサブメニュー展開機能を追加）

---

## [v1.0.1] - 2026-09-11


### Added (追加)
- **Window Position Persistence (位置記憶・自動復元)**:
  - Automatically saves the desktop widget coordinates to `%LOCALAPPDATA%\BTBatteryMonitor\settings.json` upon moving or closing.
  - Automatically restores the widget to the last saved position on startup.
  - Added safety bounds check for multi-monitor setups to prevent off-screen placement when secondary displays are disconnected.
  - （ウィジェットのドラッグ移動完了時および終了時にデスクトップ座標を自動保存し、次回起動時に正確に定位置へ復元する機能を追加。マルチモニター切断時のはみ出し防止ガード付き）

### Changed (変更)
- **Startup Registration Method (スタートアップ方式改善)**:
  - Migrated startup registration from `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` registry directly to standard Windows Startup folder (`shell:startup`) shortcuts.
  - （アンチウイルス/EDRソフトによる誤検知を回避するため、レジストリ直接書き込みを全廃し、標準のスタートアップフォルダーへのショートカット方式へ刷新）

### Fixed (修正)
- **Antivirus Silent Quarantine on PC Reboot (再起動時のファイル消滅回避)**:
  - Fixed an issue where antivirus software (e.g. F-Secure / Avira engine) flagged registry Run keys as `Drop.Win32.Startup` and silently quarantined `BTBatteryMonitor.exe` during PC startup.
  - Added automatic cleanup of legacy Run registry keys on application launch.
  - （PC再起動時にセキュリティソフトによってEXEが隔離・削除されてしまう問題を根本解決。アプリ起動時に旧レジストリ残骸を自動消去する機能を追加）

---

## [v1.0.0] - 2026-09-10

### Initial Release (初回リリース)
- **Real-Time Bluetooth Battery Monitoring**:
  - Displays real-time battery percentage of physically connected Bluetooth audio devices and peripherals on Windows 10 / 11.
  - （Windows 10 / 11 接続中の Bluetooth デバイスのバッテリー残量をリアルタイム表示）
- **Ultra-Low Resource Consumption**:
  - Event-driven battery refresh via Windows PnP message (`WM_DEVICECHANGE`) with 0.0% idle CPU overhead.
  - （OSイベント駆動による 0.0% アイドルCPU負荷）
- **Strict Connection Filtering**:
  - Excludes paired-but-disconnected or powered-off devices to keep the list clean.
  - （切断中・ペアリングのみの機器を除外する厳格な接続判定）
- **Translucent Modern Dark UI**:
  - Compact, translucent dark design matching Windows 11 Fluent UI aesthetic.
  - （Windows 11 に調和する半透明角丸ダークデザイン）
- **Custom Cyber-Styled Application Icon**:
  - Multi-resolution geometric Bluetooth & neon cyan battery icon with electric-amber heartbeat pulse.
  - （幾何学BT＋ネオンシアン＋黄金色発光パルス線のカスタムアイコン）
- **In-App Startup Toggle & Context Menu**:
  - Right-click menu with "Launch at Startup", "Always on Top", "Refresh", and "Exit".
  - （右クリックメニューから自動起動、最前面表示、手動更新、終了を設定可能）
- **User-Level Installer**:
  - Standard Inno Setup installer executable without requiring administrator privileges.
  - （管理者権限不要でインストール可能な Inno Setup インストーラーを提供）
