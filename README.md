# Bve2Tims
[BveEX](https://github.com/automatic9045/BveEX)を使ったBve5・Bve6用のプラグイン
BVEと[TIMS](https://www.led-apps.com/)をUDPで連動させる


## プラグインの機能
- [TIMS](https://www.led-apps.com/)との連動
    - UDP通信


## 導入方法
### 1. BveEXの導入
[公式のダウンロードページ](https://automatic9045.github.io/BveEX/download/)を参照してください
### 2. 本プラグインの導入
1. [Releases](releases/)から最新版がダウンロードできます
2. BveExの導入場所にある`Extensions`フォルダの中に本プラグインを配置します
    - デフォルト: `C:\Users\Public\Documents\BveEx\2.0\Extensions`
    - プラグインはBveの起動と同時に読み込まれます
        - 右クリックからBveEXの状態をみると本プラグインが正常に読み込まれているか確認できます


## 使い方
1. 右クリックメニューからBveEXの状態確認で本プラグインを有効にする
1. 右クリックメニューから送信を開始する
    - 右クリックメニューの項目チェック時に自動で送信
    - 宛先IPアドレスは`127.0.0.1`固定
    - 各種状態量を用いて自動でTIMS側と連携
        - ユニット動作状態などは完全に連動できない


## ライセンス
- [MIT](LICENSE)
    - できること
        - 商用利用
        - 修正
        - 配布
        - 私的使用
    - ダメなこと
        - 著作者は一切責任を負わない
        - 本プラグインは無保証で提供される
- 注意事項
    - 本プラグインはTIMSソフト自体とは別物です
    - 本プラグインのライセンス適用範囲はあくまで本プラグインのみであり、TIMSソフトは別にライセンスがあります


## 動作環境
- Windows
    - Win10 22H2
    - Win11 23H2 or later
- [Bve](https://bvets.net/)
    - BVE Trainsim Version 5.8.7554.391 or later
    - BVE Trainsim Version 6.0.7554.619 or later
- [BveEX](https://github.com/automatic9045/BveEX)
    - [ver2.0 - v2.0.41222.1](https://github.com/automatic9045/BveEX/releases/tag/v2.0.41222.1) or later


## 開発環境
- [BveEX](https://github.com/automatic9045/BveEX)
    - [ver2.0 - v2.0.41222.1](https://github.com/automatic9045/BveEX/releases/tag/v2.0.41222.1)
- Win10 22H2
    - Visual Studio 2022
        - Microsoft Visual Studio Community 2022 (64 ビット) - Current Version 17.5.3
- [Bve](https://bvets.net/)
    - BVE Trainsim Version 5.8.7554.391
    - BVE Trainsim Version 6.0.7554.619


## 依存環境
- BveEx.CoreExtensions (2.0.0)
    - BveEx.PluginHost (>2.0.0)

(開発者向け)  
間接参照を含めたすべての依存情報については、プロジェクトのフォルダにある `packages.lock.json` をご確認ください。
