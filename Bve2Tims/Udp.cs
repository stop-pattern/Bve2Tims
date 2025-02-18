using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bve2Tims
{
    /// <summary>
    /// UDP通信クラス
    /// 送信先とそのポートを保持する
    /// </summary>
    public class Udp : INotifyPropertyChanged
    {
        #region Static Fields

        /// <summary>
        /// 送信ポート
        /// </summary>
        private static readonly int source_port = 2331;

        /// <summary>
        /// 受信ポート
        /// </summary>
        private static readonly int destination_port = 2330;

        #endregion

        #region Fields

        /// <summary>
        /// 通信状態
        /// </summary>
        private bool status = false;

        /// <summary>
        /// 自動起動
        /// </summary>
        private bool autoStart = false;

        /// <summary>
        /// UDPクライアント
        /// </summary>
        private readonly UdpClient client;

        /// <summary>
        /// データ送信
        /// </summary>
        private IPEndPoint remoteEP;

        #endregion

        #region Properties

        /// <summary>
        /// 通信状態
        /// </summary>
        public bool Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 自動起動
        /// </summary>
        public bool AutoStart
        {
            get
            {
                return autoStart;
            }
            set
            {
                autoStart = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 送信先アドレス
        /// </summary>
        public string DestinationAddr
        {
            get
            {
                return remoteEP.Address.ToString();
            }
            set
            {
                remoteEP.Address = IPAddress.Parse(value);
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 送信先ポート
        /// </summary>
        public int DestinationPort
        {
            get
            {
                return remoteEP.Port;
            }
            set
            {
                remoteEP.Port = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Udp()
        {
            client = new UdpClient(source_port, AddressFamily.InterNetwork);
            remoteEP = new IPEndPoint(IPAddress.Loopback, destination_port);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="destination">送信先アドレス</param>
        public Udp(string destination) : this()
        {
            remoteEP.Address = IPAddress.Parse(destination);
            //Connect(destination);
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Dispose()
        {
            client.Dispose();
        }

        /// <summary>
        /// 接続先設定
        /// </summary>
        /// <param name="address">IPアドレス</param>
        public void Connect(string address)
        {
            remoteEP.Address = IPAddress.Parse(address);
        }

        /// <summary>
        /// データ送信
        /// </summary>
        /// <param name="sendBytes">送信内容</param>
        public void Send(byte[] sendBytes)
        {
            client.BeginSend(sendBytes, sendBytes.Length, remoteEP, null, null);
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

        #region INotifyPropertyChanged

        /// <summary>
        /// プロパティ変更通知イベント
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ変更通知
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
