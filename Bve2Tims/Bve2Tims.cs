using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BveEx.Extensions.Native;
using BveEx.Extensions.ContextMenuHacker;
using BveEx.PluginHost.Plugins;
using BveEx.PluginHost.Plugins.Extensions;

namespace Bve2Tims
{
    /// <summary>
    /// プラグインの本体
    /// Plugin() の第一引数でこのプラグインの仕様を指定
    /// Plugin() の第二引数でこのプラグインが必要とするBveEx本体の最低バージョンを指定（オプション）
    /// Togglable を付加するとユーザーがBveExのバージョン一覧から有効・無効を切換できる
    /// </summary>
    [Plugin(PluginType.Extension)]
    [Togglable]
    internal class ExtensionMain : AssemblyPluginBase, ITogglableExtension, IExtension
    {
        #region Plugin Settings

        /// <inheritdoc/>
        public override string Title { get; } = "TIMS連携";
        /// <inheritdoc/>
        public override string Description { get; } = "BVEとTIMSソフトを連携";

        #endregion

        #region Fields

        /// <summary>
        /// 右クリックメニュー操作用
        /// ContextMenuHacker
        /// </summary>
        private IContextMenuHacker cmx;

        /// <summary>
        /// 右クリックメニューの設定ボタン
        /// </summary>
        private ToolStripMenuItem setting;

        /// <summary>
        /// 設定ウィンドウ
        /// </summary>
        private readonly SettingWindow settingWindow;

        /// <summary>
        /// プラグインの有効・無効状態
        /// </summary>
        private bool status = true;

        /// <summary>
        /// メイン処理を受け持つクラス
        /// </summary>
        private readonly Model model;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public bool IsEnabled
        {
            get { return status; }
            set { status = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// プラグインが読み込まれた時に呼ばれる
        /// 初期化を実装する
        /// </summary>
        /// <param name="builder"></param>
        public ExtensionMain(PluginBuilder builder) : base(builder)
        {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Debug.AutoFlush = true;

            model = new Model();
            settingWindow = new SettingWindow(new ViewModel(model));
            settingWindow.Hide();

            Extensions.AllExtensionsLoaded += AllExtensionsLoaded;
            settingWindow.Closing += SettingWindowClosing;
        }

        #endregion

        #region Inheritance Methods

        /// <summary>
        /// プラグインが解放されたときに呼ばれる
        /// 後処理を実装する
        /// </summary>
        public override void Dispose()
        {
            settingWindow.Close();

            settingWindow.Closing -= SettingWindowClosing;
            Extensions.AllExtensionsLoaded -= AllExtensionsLoaded;
            setting.CheckedChanged -= MenuItemCheckedChanged;
        }

        /// <summary>
        /// シナリオ読み込み中に毎フレーム呼び出される
        /// </summary>
        /// <param name="elapsed">前回フレームからの経過時間</param>
        public override void Tick(TimeSpan elapsed)
        {
            if (status)
            {
                model.Tick();
            }
        }

        #endregion

        #region Eevent Handlers
        
        /// <summary>
        /// すべての拡張機能が読み込まれたときに呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        private void AllExtensionsLoaded(object sender, EventArgs e)
        {
            cmx = Extensions.GetExtension<IContextMenuHacker>();

            model.Initialize(BveHacker,  Extensions.GetExtension<INative>());

            setting = cmx.AddCheckableMenuItem("TIMS連携設定", MenuItemCheckedChanged, ContextMenuItemType.CoreAndExtensions);
#if DEBUG
            setting.Checked = true;
#endif
        }

        /// <summary>
        /// メニューのチェック状態が変更されたときに呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        private void MenuItemCheckedChanged(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                if (item.Checked)
                    settingWindow.Show();
                else
                    settingWindow.Hide();
            }
        }

        /// <summary>
        /// 設定ウィンドウが閉じられたときに呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void SettingWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is SettingWindow window)
            {
                e.Cancel = true;
                window.Hide();
                setting.Checked = false;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
