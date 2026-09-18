# BTBatteryMonitor

[English](README_en.md) | [日本語](README.md)

Windows 10 / 11 向けの Bluetooth バッテリー残量表示ウィジェット。  
PCに接続されているBluetooth機器のバッテリー残量を、デスクトップ上に常時表示します。

<p align="center">
  <img src="docs/images/screenshot.png" alt="BTBatteryMonitor Widget Preview" width="300" />
</p>

---

## 主な機能

- **バッテリー残量の表示**: 接続中のBluetooth機器（ヘッドホン、マウス、キーボード等）のバッテリー残量を一覧表示。
- **イベント駆動更新**: OSのデバイス接続・切断イベント（`WM_DEVICECHANGE`）を検知して自動更新（低CPU負荷）。
- **ドラッグ移動 ＆ 位置記憶**: デスクトップ上の好きな位置へ配置可能。配置位置は次回起動時にも自動復元。
- **外観のカスタマイズ**: 右クリックメニューから本体色（プリセット＋カラーパレット）および透明度を変更可能。
- **スタートアップ起動**: Windows起動時の自動起動に対応（スタートアップフォルダーへのショートカット配置）。

---

## 動作環境

- **OS**: Windows 10 (Build 19041 以上) / Windows 11 (x64)
- **Bluetooth**: Bluetooth 4.0 以上

---

## インストール方法

### インストーラー版
1. [Releases](https://github.com/DT-AIRA/BTBatteryMonitor/releases) から `BTBatteryMonitor_Setup.exe` をダウンロードして実行します。
2. 画面の指示に従ってインストールします（管理者権限は不要です）。

### ポータブル版
1. [Releases](https://github.com/DT-AIRA/BTBatteryMonitor/releases) から `BTBatteryMonitor.exe` をダウンロードします。
2. 任意のフォルダーに配置し、ダブルクリックして起動します。

---

## 使い方

- **ウィジェットの移動**: ウィジェット上を左ドラッグすると移動できます。
- **右クリックメニュー**:
  - **Refresh**: バッテリー残量を手動で再取得します。
  - **Widget Color**: 本体の背景色を変更します（5色のプリセット、またはカラーパレットから自由選択）。
  - **Widget Opacity**: 本体の透明度を変更します（100% / 85% / 65% / 45% / 25%）。
  - **Always on Top**: ウィンドウの最前面表示を切り替えます。
  - **Launch at Startup**: Windows起動時の自動実行を切り替えます。
  - **Exit**: アプリケーションを終了します。

---

## ビルド手順

```bat
setup_env.bat   # ローカル開発環境のセットアップ（.NET 8 SDK 展開）
build.bat       # ビルド
run.bat         # 実行
publish.bat     # リリース用パッケージ作成（単一EXE＋インストーラー生成）
```

---

## 技術スタック

- **フレームワーク**: WPF (.NET 8 Windows Desktop / x64)
- **API**: Win32 SetupAPI / WinRT (`Windows.Devices.Bluetooth`, `Windows.Devices.Enumeration`)
- **インストーラー**: Inno Setup 6

---

## ライセンス

Copyright (c) 2026 DT-AIRA  
本ソフトウェアは [Apache License, Version 2.0](LICENSE) の下で公開されています。
