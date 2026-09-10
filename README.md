# BTBatteryMonitor

[English](README_en.md) | [日本語](README.md)

Windows 10 / 11 向けの軽量・低負荷な常駐型 Bluetooth バッテリー残量表示ウィジェット。  
現在接続されているBluetooth機器のバッテリー残量を、デスクトップ上にコンパクトかつ透過Fluentデザインでリアルタイム表示します。

<p align="center">
  <img src="docs/images/screenshot.png" alt="BTBatteryMonitor Widget Preview" width="300" />
</p>

---

## 主な特徴

- **リアルタイム検知**:
  - OSのデバイス変更イベント（`WM_DEVICECHANGE`）をフックし、Bluetooth機器の接続・切断を即座にUIへ反映。
  - 常時ポーリングを行わないため、待機時のCPU使用率は実質 **0.0%** を維持します。
- **厳密な物理接続判定**:
  - Windows公式APIを通じて実際に物理接続されている機器のみを厳密に抽出。
  - ペアリング済みの過去の履歴や電源オフの機器は自動的に除外されます。
- **高密度＆Fluentデザイン**:
  - デスクトップに美しく馴染む半透明スモーキーブラック背景（角丸・枠なし）。
  - デバイス種別（ヘッドホン、マウス、キーボード、ゲームパッド）に応じたスカイブルーのモダンアイコン。
  - バッテリー残量に応じた動的な横向き電池バー（残量20%以下で赤色警告）。
  - ドラッグ＆ドロップでデスクトップ上の好きな位置へ自由に配置可能。
- **二重起動防止**:
  - 多重起動防止機能により、常に1つのインスタンスのみが安全に常駐。
- **完全自己完結ビルド環境**:
  - OSにグローバルな開発環境を導入することなく、プロジェクトフォルダ内でSDKの取得からビルド、インストーラー生成まで完結。

---

## 動作環境

- **OS**: Windows 10 (Build 19041 以上) / Windows 11 (x64)
- **Bluetooth**: Bluetooth 4.0 以上（BLE / Classic Bluetooth 双方対応）

---

## インストール方法

### 方法1: インストーラー（推奨）
1. [Releases](https://github.com/DT-AIRA/BTBatteryMonitor/releases) から最新の `BTBatteryMonitor_Setup.exe` をダウンロードして実行します。
2. 画面の指示に従ってインストールします（管理者権限は不要です）。
3. インストール時のオプションで「Windows起動時に自動起動」を選択すると、スタートアップに登録され、PC起動時に自動でバックグラウンド常駐します。
4. アンインストールは、Windowsの「設定 > アプリ」またはコントロールパネルからいつでも安全に行えます。

### 方法2: ポータブル版
1. [Releases](https://github.com/DT-AIRA/BTBatteryMonitor/releases) から `BTBatteryMonitor.exe` をダウンロードします。
2. 任意のフォルダに配置し、ダブルクリックして起動するだけで利用可能です。

---

## 使い方

- **ウィジェットの移動**: ウィジェット上の任意の部分を左ドラッグすると、画面内の好きな位置へ移動できます。
- **右クリックメニュー**:
  - **Always on Top (最前面に表示)**: ウィンドウを常に最前面に固定するかどうかを切り替えます（チェックマーク連動）。
  - **Refresh (手動更新)**: バッテリー残量を即座に再取得します。
  - **Exit (終了)**: アプリケーションを終了します。

---

## 開発・ビルド手順

システム全体に .NET SDK や Inno Setup をインストールすることなく、バッチスクリプトを実行するだけでローカルに環境が構築されます。

```bat
# 1. 開発環境のセットアップ（プロジェクトローカルに .NET 8 SDK を展開）
setup_env.bat

# 2. ビルド
build.bat

# 3. 実行（デバッグ起動）
run.bat

# 4. 配布パッケージ作成（単一EXE化 + インストーラー生成）
publish.bat
```
- ビルドされた単一実行可能ファイルは `publish\BTBatteryMonitor.exe` に出力されます。
- 生成されたインストーラーは `dist\BTBatteryMonitor_Setup.exe` に出力されます。

---

## アーキテクチャと技術スタック

- **UI Framework**: WPF (.NET 8 Windows Desktop / x64)
- **Windows APIs**:
  - `setupapi.dll` (Win32 PnP Property Query: Classic Bluetooth / Hands-Free オーディオプロパティ `{104EA319...} 2` 直読み)
  - `Windows.Devices.Enumeration` & `Windows.Devices.Bluetooth` (WinRT: BLE AEP バッテリー・MACアドレス判定)
  - `HwndSource` / `WM_DEVICECHANGE` (Windowsメッセージフックによるイベント駆動型リアルタイム更新)
- **Packaging**: Inno Setup 6 (ポータブルコンパイラ経由)

---

## ライセンス

Copyright (c) 2026 DT-AIRA

本ソフトウェアは [Apache License, Version 2.0](LICENSE) の下で公開されています。
