using System;
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
    public class Model
    {
        #region const/static Fields

        public static int originPort = 0;
        public static int destinationPort = 0;

        private const int unitNumber = 3;
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

        private bool autoStart = true;

        private bool status = false;

        private int selectedDestinationIndex;

        private Udp selectedDestination;

        private ObservableCollection<Udp> destinations = new ObservableCollection<Udp>();

        /// <summary>
        /// Native
        /// </summary>
        private INative native;

        /// <summary>
        /// UDP
        /// </summary>
        private Udp udpControl;

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

        internal Model()
        {
            destinations = new ObservableCollection<Udp>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="native"><see cref="INative"/></param>
        internal void Initialize(INative native)
        {
            if (native == null)
            {
                throw new ArgumentNullException(nameof(native));
            }

            if (this.native == null)
            {
                native.Opened -= NativeOpened;
                native.Closed -= NativeClosed;
            }

            this.native = native;

            native.Opened += NativeOpened;
            native.Closed += NativeClosed;
        }

        internal void Start()
        {
            status = true;
        }

        internal void Stop()
        {
            status = false;
        }

        /// <summary>
        /// 送信処理
        /// </summary>
        internal void Tick()
        {
            var message = GetVehicleState();
            Debug.WriteLine($"Message: {message}");
            foreach (var dest in destinations)
            {
                if (dest.Status)
                {
                    Debug.WriteLine($"Send: {dest.DestinationAddr}:{dest.DestinationPort}");
                    dest.Send(message);
                }
            }
        }

        internal void Send(string data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            foreach (Udp udp in destinations)
            {
                udp.Item1.Send(buffer, buffer.Length, udp.Item2);
            }
        }

        /// <summary>
        /// 送信文字列取得
        /// </summary>
        /// <returns>送信文字列</returns>
        internal string GetVehicleState()
        {
            if (status)
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
                return message;
            }
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

        #endregion

        #region Eevent Handlers

        /// <summary>
        /// <see cref="Native"/> が利用可能になったときに呼ばれる
        /// </summary>
        private void NativeOpened(object sender, EventArgs e)
        {

            udpControl = new Udp();
            canSend = true;
        }

        /// <summary>
        /// <see cref="Native"/> が利用不能になる直前に呼ばれる
        /// </summary>
        private void NativeClosed(object sender, EventArgs e)
        {
            canSend = false;
        }

        #endregion
    }
}
