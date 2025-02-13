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

        public int originPort = 0;
        public int destinationPort = 0;

        private const int unitNumber = 3;
        private const int doorNumber = 10;

        private static int[] unitIndexes = new int[unitNumber];
        private static int[] doorIndexes = new int[doorNumber];

        #endregion

        #region Fields

        private bool autoStart = true;

        private bool status = false;
        
        private UdpClient udpClient;

        private IPEndPoint remoteEP;

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
        /// 宛先アドレス
        /// </summary>
        internal string Address
        {
            get
            {
                return remoteEP.Address.ToString();
            }
            set
            {
                remoteEP.Address = IPAddress.Parse(value);
            }
        }

        /// <summary>
        /// 宛先ポート番号
        /// </summary>
        internal int Port
        {
            get
            {
                return remoteEP.Port;
            }
            set
            {
                remoteEP.Port = value;
            }
        }

        #endregion

        #region Constructors

        internal Model()
        {
            udpClient = new UdpClient();
            remoteEP = new IPEndPoint(IPAddress.Loopback, destinationPort);
        }

        internal Model(int originPort, int destinationPort)
        {
            this.originPort = originPort;
            this.destinationPort = destinationPort;
            udpClient = new UdpClient(originPort);
            remoteEP = new IPEndPoint(IPAddress.Loopback, destinationPort);
        }

        internal Model(int originPort, int destinationPort, string address)
        {
            this.originPort = originPort;
            this.destinationPort = destinationPort;
            udpClient = new UdpClient(originPort);
            remoteEP = new IPEndPoint(IPAddress.Parse(address), destinationPort);
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
            udpClient.SendAsync(buffer, buffer.Length, remoteEP);
        }

        internal byte[] Receive()
        {
            return udpClient.Receive(ref remoteEP);
        }

        #endregion
    }
}
