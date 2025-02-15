﻿using System;
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
using BveTypes.ClassWrappers;

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
        private SettingWindow settingWindow;

        /// <summary>
        /// プラグインの有効・無効状態
        /// </summary>
        private bool status = true;

        /// <summary>
        /// Native
        /// </summary>
        private INative native;

        /// <summary>
        /// UDP
        /// </summary>
        private UdpControl udpControl;

        /// <summary>
        /// 送信可能かどうか
        /// </summary>
        private bool canSend = false;

        /// <summary>
        /// ユニットのpanelインデックス
        /// </summary>
        private readonly int[] unitIndexes = new int[] { -1, -1, -1 };
        //private readonly int[] unitIndexes = new int[] { 213, 214, 215 };

        /// <summary>
        /// ドアのpanelインデックス
        /// </summary>
        private readonly int[] doorIndexes = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

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

            Extensions.AllExtensionsLoaded += AllExtensionsLoaded;

            settingWindow = new SettingWindow();
            settingWindow.Closing += SettingWindowClosing;
            settingWindow.Hide();
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

            setting.CheckedChanged -= MenuItemCheckedChanged;
            native.Opened -= NativeOpened;
            native.Closed -= NativeClosed;

            udpControl.Dispose();
            udpControl = null;
            native = null;
        }

        /// <summary>
        /// シナリオ読み込み中に毎フレーム呼び出される
        /// </summary>
        /// <param name="elapsed">前回フレームからの経過時間</param>
        public override void Tick(TimeSpan elapsed)
        {
            if (status && canSend)
            {
                string message = $"{native.VehicleState.Location},{native.VehicleState.Speed},{(int)native.VehicleState.Time.TotalMilliseconds},";
                //string message = "";
                //message += native.VehicleState.Location.ToString() + ",";
                //message += native.VehicleState.Speed.ToString() + ",";
                //message += native.VehicleState.Time.TotalMilliseconds.ToString();

                // Unit表示
                foreach (var index in unitIndexes)
                {
                    if (index > 0)
                    {
                        message += $"{GetPanelData(index)},";
                    }
                    else
                    {
                        var current = BveHacker.Scenario.Vehicle.Instruments.Electricity.MotorState.Current;
                        if (current > 0)
                        {
                            message += "1,";
                        }
                        else if (current < 0)
                        {
                            message += "2,";
                        }
                        else
                        {
                            message += "0,";
                        }
                    }
                }

                // ドア表示
                for (int i = 0; i < doorIndexes.Length; i++)
                {
                    if (doorIndexes.ElementAt(i) > 0)
                    {
                        message += $"{GetPanelData(doorIndexes.ElementAt(i))},";
                        continue;
                    }
                    else
                    {
                        DoorSet ds = BveHacker.Scenario.Vehicle.Doors;
                        if (ds.AreAllClosed)
                        {
                            message += "0,";
                            continue;
                        }
                        else
                        {
                            int door = 0;
                            try
                            {
                                if (ds.GetSide(DoorSide.Right).CarDoors.ElementAt(i).IsOpen)
                                {
                                    door = 1;
                                }
                                else if (ds.GetSide(DoorSide.Left).CarDoors.ElementAt(i).IsOpen)
                                {
                                    door = 2;
                                }
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                Debug.WriteLine("ArgumentOutOfRangeException: car number is out of range");
                                door = 0;
                            }
                            catch (Exception)
                            {
                                door = 0;
                            }
                            message += $"{door},";
                        }
                    }
                }

                Debug.WriteLine($"Send: {message}");
                udpControl.Send(message);
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
            native = Extensions.GetExtension<INative>();
            cmx = Extensions.GetExtension<IContextMenuHacker>();

            native.Opened += NativeOpened;
            native.Closed += NativeClosed;

            setting = cmx.AddCheckableMenuItem("TIMS連携設定", MenuItemCheckedChanged, ContextMenuItemType.CoreAndExtensions);
        }

        /// <summary>
        /// <see cref="Native"/> が利用可能になったときに呼ばれる
        /// </summary>
        private void NativeOpened(object sender, EventArgs e)
        {
            udpControl = new UdpControl();
            canSend = true;
        }

        /// <summary>
        /// <see cref="Native"/> が利用不能になる直前に呼ばれる
        /// </summary>
        private void NativeClosed(object sender, EventArgs e)
        {
            canSend = false;
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

        #region Methods

        /// <summary>
        /// パネルデータ取得
        /// </summary>
        /// <param name="index">パネルのindex</param>
        /// <returns></returns>
        private int GetPanelData(int index)
        {
            if (index < 0)
            {
                return 0;
            }

            try
            {
                return native.AtsPanelArray.ElementAt(index);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine(ex.Message);
                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return 0;
            }
        }

        #endregion
    }

    /// <summary>
    /// UDP通信クラス
    /// </summary>
    internal class UdpControl
    {
        #region Fields

        /// <summary>
        /// 送信ポート
        /// </summary>
        private static int source_port = 2331;

        /// <summary>
        /// 受信ポート
        /// </summary>
        private static int destination_port = 2330;

        /// <summary>
        /// 送信先アドレス
        /// </summary>
        private string destination_addr = "127.0.0.1";

        /// <summary>
        /// UDPクライアント
        /// </summary>
        private UdpClient client = new UdpClient(source_port, AddressFamily.InterNetwork);

        /// <summary>
        /// データ送信
        /// </summary>
        private IPEndPoint remoteEP = new IPEndPoint(IPAddress.Loopback, destination_port);

        #endregion

        #region Methods

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UdpControl()
        {
            remoteEP = new IPEndPoint(IPAddress.Loopback, destination_port);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="destination">送信先アドレス</param>
        public UdpControl(string destination) : this()
        {
            //Connect(destination);
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Dispose()
        {
            client.Close();
        }

        /// <summary>
        /// 接続先設定
        /// </summary>
        /// <param name="address">IPアドレス</param>
        public void Connect(string address)
        {
            destination_addr = address;
            remoteEP = new IPEndPoint(IPAddress.Parse(destination_addr), destination_port);
        }

        /// <summary>
        /// データ送信
        /// </summary>
        /// <param name="message">送信内容</param>
        public void Send(string message)
        {
            byte[] sendBytes = Encoding.UTF8.GetBytes(message);
            //client.Connect(remoteEP);
            //client.Send(sendBytes, sendBytes.Length);
            //client.Close();
            client.BeginSend(sendBytes, sendBytes.Length, remoteEP, null, null);
        }

        #endregion
    }

}
