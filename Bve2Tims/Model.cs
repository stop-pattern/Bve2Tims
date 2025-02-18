﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BveEx.Extensions.Native;
using BveEx.PluginHost;
using BveTypes.ClassWrappers;

namespace Bve2Tims
{
    /// <summary>
    /// メイン処理を受け持つクラス
    /// シングルトンではないがインスタンスは1つしか生成しない
    /// </summary>
    public class Model: IDisposable
    {
        #region const/static Fields

        /// <summary>
        /// ユニット数
        /// </summary>
        private const int unitNumber = 3;

        /// <summary>
        /// 両数
        /// </summary>
        private const int doorNumber = 10;

        /// <summary>
        /// ユニットのpanelインデックス
        /// </summary>
        private static int[] unitIndexes = new int[unitNumber] { -1, -1, -1 };
        //private readonly int[] unitIndexes = new int[unitNumber] { 213, 214, 215 };

        /// <summary>
        /// ドアのpanelインデックス
        /// </summary>
        private static int[] doorIndexes = new int[doorNumber] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

        #endregion

        #region Fields

        /// <summary>
        /// 選択中の宛先インデックス
        /// </summary>
        private int selectedDestinationIndex;

        /// <summary>
        /// 選択中の宛先
        /// </summary>
        private Udp selectedDestination;

        /// <summary>
        /// 宛先リスト
        /// </summary>
        private ObservableCollection<Udp> destinations;

        /// <summary>
        /// BveHacker
        /// </summary>
        private IBveHacker bveHacker;

        /// <summary>
        /// Native
        /// </summary>
        private INative native;

        /// <summary>
        /// 送信可能か
        /// </summary>
        private bool canSend = false;

        #endregion

        #region Properties

        /// <summary>
        /// ユニットインデックス
        /// </summary>
        internal static int[] UnitIndexes
        {
            get
            {
                return unitIndexes;
            }
            set
            {
                unitIndexes = value;
            }
        }

        /// <summary>
        /// ドアインデックス
        /// </summary>
        internal static int[] DoorIndexes
        {
            get
            {
                return doorIndexes;
            }
            set
            {
                doorIndexes = value;
            }
        }

        /// <summary>
        /// 選択中の宛先インデックス
        /// </summary>
        internal int SelectedDestinationIndex
        {
            get
            {
                return selectedDestinationIndex;
            }
            set
            {
                selectedDestinationIndex = value;
            }
        }

        /// <summary>
        /// 選択中の宛先
        /// </summary>
        internal Udp SelectedDestination
        {
            get
            {
                return selectedDestination;
            }
            set
            {
                selectedDestination = value;
            }
        }

        /// <summary>
        /// 宛先リスト
        /// </summary>
        internal ObservableCollection<Udp> Destinations
        {
            get
            {
                return destinations;
            }
            set
            {
                destinations = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Model()
        {
            destinations = new ObservableCollection<Udp> { new Udp() };
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Model(List<Udp> udps)
        {
            if (udps == null)
            {
                throw new ArgumentNullException(nameof(udps));
            }

            if (udps.Count == 0)
            {
                udps.Add(new Udp());
            }
            destinations = new ObservableCollection<Udp>(udps);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="hacker"><see cref="IBveHacker"/> インスタンス</param>
        /// <param name="native"><see cref="INative"/> インスタンス</param>
        internal void Initialize(IBveHacker hacker, INative native)
        {
            if (hacker == null)
            {
                throw new ArgumentNullException(nameof(hacker));
            }
            else if (native == null)
            {
                throw new ArgumentNullException(nameof(native));
            }

            if (this.native == null)
            {
                native.Opened -= NativeOpened;
                native.Closed -= NativeClosed;
            }

            this.bveHacker = hacker;
            this.native = native;

            native.Opened += NativeOpened;
            native.Closed += NativeClosed;
        }

        /// <summary>
        /// 送信処理
        /// </summary>
        internal void Tick()
        {
            if (canSend)
            {
                string message = GetVehicleState();
                byte[] sendBytes = Encoding.UTF8.GetBytes(message);
                Debug.WriteLine($"Message: {message}");

                foreach (var dest in destinations)
                {
                    if (dest.Status)
                    {
                        dest.Send(sendBytes);
                        Debug.WriteLine($"Send: {dest.DestinationAddr}:{dest.DestinationPort}");
                    }
                }
            }
        }

        /// <summary>
        /// 送信文字列取得
        /// </summary>
        /// <returns>送信文字列</returns>
        internal string GetVehicleState()
        {
            string message = $"{native.VehicleState.Location},{native.VehicleState.Speed},{(int)native.VehicleState.Time.TotalMilliseconds},";
            //string message = "";
            //message += native.VehicleState.Location.ToString() + ",";
            //message += native.VehicleState.Speed.ToString() + ",";
            //message += native.VehicleState.Time.TotalMilliseconds.ToString();

            // Unit表示
            float current = native.VehicleState.Current;
            foreach (var index in unitIndexes)
            {
                if (index > 0)
                {
                    message += $"{GetPanelData(index)},";
                }
                else
                {
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
            DoorSet ds = bveHacker.Scenario.Vehicle.Doors;
            for (int i = 0; i < doorIndexes.Length; i++)
            {
                if (doorIndexes.ElementAt(i) > 0)
                {
                    message += $"{GetPanelData(doorIndexes.ElementAt(i))},";
                    continue;
                }
                else
                {
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
            return message;
        }

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

        /// <summary>
        /// 送受信状態を設定
        /// </summary>
        /// <param name="status">状態</param>
        public void SetStatus(bool status)
        {
            foreach (var dest in destinations)
            {
                dest.Status = status;
            }
        }

        #endregion

        #region Eevent Handlers

        /// <summary>
        /// <see cref="Native"/> が利用可能になったときに呼ばれる
        /// </summary>
        private void NativeOpened(object sender, EventArgs e)
        {
            canSend = true;

            foreach (var dest in destinations)
            {
                if (dest.AutoStart)
                {
                    dest.Status = true;
                }
            }   
        }

        /// <summary>
        /// <see cref="Native"/> が利用不能になる直前に呼ばれる
        /// </summary>
        private void NativeClosed(object sender, EventArgs e)
        {
            canSend = false;

            foreach (var dest in destinations)
            {
                if (dest.AutoStart)
                {
                    dest.Status = false;
                }
            }
        }

        #endregion

        #region IDisposable Support

        /// <summary>
        /// 重複する呼び出しを検出する
        /// </summary>
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (destinations != null || destinations.Count == 0)
                    {
                        foreach (var dest in destinations)
                        {
                            dest.Dispose();
                        }
                    }
                    native.Opened -= NativeOpened;
                    native.Closed -= NativeClosed;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
