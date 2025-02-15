using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Bve2Tims
{
    internal class Model
    {
        #region const/static Fields

        public static int originPort = 0;
        public static int destinationPort = 0;

        private const int unitNumber = 3;
        private const int doorNumber = 10;

        private static int[] unitIndexes = new int[unitNumber];
        private static int[] doorIndexes = new int[doorNumber];

        #endregion

        #region Fields

        private bool autoStart = true;

        private bool status = false;

        private int selectedDestinationIndex;

        private Udp selectedDestination;

        private List<Udp> destinations = new List<Udp>();

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
        /// 自動起動
        /// </summary>
        internal bool AutoStart
        {
            get
            {
                return autoStart;
            }
            set
            {
                autoStart = value;
            }
        }

        /// <summary>
        /// 通信状態
        /// </summary>
        internal bool Status
        {
            get
            {
                return status;
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
        internal List<Udp> Destinations
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
            destinations = new List<Udp>();
        }

        #endregion

        #region Methods

        internal void Start()
        {
            status = true;
        }

        internal void Stop()
        {
            status = false;
        }

        internal void Send(string data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            foreach (Udp udp in destinations)
            {
                udp.Item1.Send(buffer, buffer.Length, udp.Item2);
            }
        }

        #endregion
    }
}
