using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bve2Tims
{
    /// <summary>
    /// UDP通信クラス
    /// 送信先とそのポートを保持する
    /// </summary>
    internal class Udp
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
        public Udp()
        {
            remoteEP = new IPEndPoint(IPAddress.Loopback, destination_port);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="destination">送信先アドレス</param>
        public Udp(string destination) : this()
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
