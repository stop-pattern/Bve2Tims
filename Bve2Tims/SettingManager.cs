using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bve2Tims
{
    /// <summary>
    /// 設定ファイルのシリアライズ用クラス
    /// </summary>
    [XmlRoot]
    public class Settings
    {
        /// <summary>
        /// ユニットのインデックス
        /// </summary>
        [XmlArray]
        [XmlArrayItem("Index")]
        public int[] UnitIndexes { get; set; }

        /// <summary>
        /// ドアのインデックス
        /// </summary>
        [XmlArray]
        [XmlArrayItem("Index")]
        public int[] DoorIndexes { get; set; }

        /// <summary>
        /// UDP通信の設定
        /// </summary>
        [XmlArray]
        [XmlArrayItem("Index")]
        public Udp[] Udps { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public Settings()
        {
            UnitIndexes = new int[0];
            DoorIndexes = new int[0];
            Udps = new Udp[0];
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="door"></param>
        /// <param name="udp"></param>
        public Settings(int[] unit, int[] door, Collection<Udp> udp)
        {
            UnitIndexes = unit;
            DoorIndexes = door;
            Udps = udp.ToArray();
        }

        /// <summary>
        /// <see cref="Model"/> から変換
        /// </summary>
        /// <param name="model">変換元の <see cref="Model"/> インスタンス</param>
        /// <returns></returns>
        public static Settings FromModel(Model model)
        {
            return new Settings(Model.UnitIndexes.ToArray(), Model.DoorIndexes.ToArray(), model.Destinations);
        }

        /// <summary>
        /// <see cref="Model"/> に変換
        /// </summary>
        /// <returns>変換元の <see cref="Model"/> インスタンス</returns>
        public static Model ToModel(Settings settings)
        {
            Model.UnitIndexes = settings.UnitIndexes;
            Model.DoorIndexes = settings.DoorIndexes;
            return new Model(settings.Udps);
        }
    }

    class SettingManager
    {
    }
}
